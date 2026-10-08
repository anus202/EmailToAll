using EmailToAll.Data;
using EmailToAll.Data.Interface;
using EmailToAll.Data.Repository;
using EmailToAll.Models;
using EmailToAll.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Refuse to start outside Development while secrets are still the placeholders from appsettings.json.
if (!builder.Environment.IsDevelopment())
{
    string[] requiredSecrets =
    {
        "JwtSettings:SecretKey",
        "Encryption:Key",
        "Encryption:IV",
        "EmailSettings:Password"
    };

    foreach (var key in requiredSecrets)
    {
        var value = builder.Configuration[key];
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("REPLACE_ME", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Configuration value '{key}' is not set. Provide it through user-secrets or environment variables.");
        }
    }
}

// Add Controllers (REST API only; the UI lives in /frontend)
builder.Services.AddControllers();

// Database Context
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Conn")));

// Dependency Injection (Services & Repositories)
builder.Services.AddScoped<ISignupRepository, SignupRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();
builder.Services.AddScoped<IEmailSetupRepository, EmailSetupRepository>();
builder.Services.AddScoped<IEmailHistoryRepository, EmailHistoryRepository>();
builder.Services.AddScoped<IEncryptionRepository, EncryptionService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SignupService>();
builder.Services.AddHostedService<OtpExpiryService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<SmtpProviderService>();
builder.Services.AddScoped<EmailSetupService>();
builder.Services.AddScoped<SendEmailService>();
builder.Services.AddScoped<EmailHistoryService>();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Swagger / OpenAPI Setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Authentication & JWT Setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwtToken"))
            {
                context.Token = context.Request.Cookies["jwtToken"];
            }
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            // This is an API: answer 401 so the frontend can send the user back to the login page.
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Unauthorized" });
        }
    };
});

var app = builder.Build();

// Auto Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

// Development Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 API");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
