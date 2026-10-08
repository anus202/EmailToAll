# EmailToAll

[![build](https://github.com/anus202/EmailToAll/actions/workflows/build.yml/badge.svg)](https://github.com/anus202/EmailToAll/actions/workflows/build.yml)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

ASP.NET Core Web API for user signup and login with **email OTP verification** and **JWT authentication**.
One-time codes are delivered over SMTP with [MailKit](https://github.com/jstedfast/MailKit), users are stored in SQL Server through Entity Framework Core, and every endpoint is documented in Swagger.

## Features

- **Signup** with email, password and password confirmation
- **Email OTP verification** — 6-digit code, valid for 5 minutes, with a resend endpoint
- **Login** that issues a signed JWT (read from the `Authorization: Bearer` header or the `jwtToken` cookie)
- **Forgot / reset password** flow protected by an emailed OTP
- **Background cleanup** — a hosted service marks expired OTPs as used every minute
- **Swagger UI** with Bearer-token support
- **Automatic EF Core migrations** applied on startup
- Repository + Unit of Work data layer, and a configurable CORS policy

## Tech stack

| Area           | Technology                                        |
| -------------- | ------------------------------------------------- |
| Framework      | ASP.NET Core 8 (Web API + MVC views)              |
| Data access    | Entity Framework Core 8, SQL Server               |
| Authentication | JWT Bearer (`System.IdentityModel.Tokens.Jwt`)    |
| Email          | MailKit / MimeKit over SMTP (STARTTLS)            |
| API docs       | Swashbuckle (Swagger / OpenAPI)                   |

## Project structure

```
EmailToAll
├── EmailToAll.sln
└── src/EmailToAll
    ├── Controllers/     HTTP endpoints (Signup, Login, Home)
    ├── Service/         Business logic (signup, login, JWT, email, encryption, OTP expiry job)
    ├── Data/            Repository interfaces/implementations and Unit of Work
    ├── Models/          EF Core entities and DbContext
    ├── Payload/         Request DTOs
    ├── Migrations/      EF Core migrations
    ├── Views/           Razor views
    └── wwwroot/         Static assets
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server — the default connection string targets **LocalDB** (`(localdb)\MSSQLLocalDB`)
- An SMTP account. For Gmail, create an [app password](https://support.google.com/accounts/answer/185833)

### 1. Clone

```bash
git clone https://github.com/anus202/EmailToAll.git
cd EmailToAll
```

### 2. Configure secrets

Secrets are **not** stored in the repository. Set them with [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (or environment variables):

```bash
cd src/EmailToAll
dotnet user-secrets set "EmailSettings:SenderEmail" "you@gmail.com"
dotnet user-secrets set "EmailSettings:Password"    "<smtp app password>"
dotnet user-secrets set "JwtSettings:SecretKey"     "<random string, 32+ characters>"
dotnet user-secrets set "Encryption:Key"            "<exactly 32 characters>"
dotnet user-secrets set "Encryption:IV"             "<exactly 16 characters>"
```

Outside the `Development` environment the app refuses to start while any of these still hold the `REPLACE_ME` placeholders from `appsettings.json`.

### 3. Run

```bash
dotnet run --project src/EmailToAll
```

The database and tables are created automatically on first start. Open **http://localhost:5018/swagger** to explore the API (or run with `--launch-profile https` and use **https://localhost:7060/swagger**).

## Configuration

| Key                                | Description                                                  |
| ---------------------------------- | ------------------------------------------------------------ |
| `ConnectionStrings:Conn`           | SQL Server connection string                                 |
| `EmailSettings:SmtpServer` / `Port`| SMTP host and port (STARTTLS)                                |
| `EmailSettings:SenderName`         | Display name on outgoing mail                                |
| `EmailSettings:SenderEmail`        | SMTP username and "from" address                             |
| `EmailSettings:Password`           | SMTP password / app password                                 |
| `JwtSettings:SecretKey`            | HMAC-SHA256 signing key (32+ characters)                     |
| `JwtSettings:Issuer` / `Audience`  | Token issuer and audience                                    |
| `JwtSettings:ExpiryMinutes`        | Token lifetime in minutes                                    |
| `Encryption:Key` / `IV`            | AES key (32 chars) and IV (16 chars) used for stored passwords |
| `Cors:AllowedOrigins`              | Origins allowed by the `AllowFrontend` CORS policy           |

Nested keys map to environment variables with a double underscore, e.g. `JwtSettings__SecretKey`.

## API reference

All endpoints accept and return JSON. Responses follow the shape `{ "success": bool, "message": string?, "data": any? }`.

| Method | Endpoint                          | Body                                  | Description                                  |
| ------ | --------------------------------- | ------------------------------------- | -------------------------------------------- |
| POST   | `/api/Signup/Signup`              | `Email`, `Password`, `ConfirmPass`    | Register a user and email a verification OTP |
| POST   | `/api/Signup/OptVerification`     | `Email`, `Token`                      | Verify the signup OTP                        |
| POST   | `/api/Signup/resendOtp`           | `Email`                               | Invalidate the previous OTP and send a new one |
| POST   | `/api/Login/login`                | `Email`, `Password`                   | Authenticate and receive a JWT               |
| POST   | `/api/Login/ForgetPassword`       | `Email`                               | Email a password-reset OTP                   |
| POST   | `/api/Login/LoginOptVerification` | `Email`, `Token`                      | Verify the password-reset OTP                |
| POST   | `/api/Login/PasswordReset`        | `Email`, `Password`                   | Set a new password                           |

### Typical flow

1. `POST /api/Signup/Signup` — an OTP arrives by email.
2. `POST /api/Signup/OptVerification` with the code.
3. `POST /api/Login/login` — copy the returned `token`.
4. In Swagger UI click **Authorize** and paste the token to call protected endpoints.

## Contributing

Issues and pull requests are welcome. Please open an issue first to discuss larger changes.

## License

Released under the [MIT License](LICENSE).
