# EmailToAll

[![build](https://github.com/anus202/EmailToAll/actions/workflows/build.yml/badge.svg)](https://github.com/anus202/EmailToAll/actions/workflows/build.yml)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![React](https://img.shields.io/badge/React-19-61DAFB)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

Write one email and send it to many people at once, from **your own mailbox**.

EmailToAll is a full-stack app: an **ASP.NET Core 8 REST API** (SQL Server, JWT authentication, email OTP verification) and a **React + TypeScript** frontend (`/frontend`). Each user connects their own email account, composes a message with To / Cc / Bcc recipients and attachments, and keeps a history of everything that was sent.

## Features

**For users**

- Sign up with email OTP verification, sign in, and reset a forgotten password
- **Email Setup** — connect your own mailbox (Gmail, Outlook, Yahoo, iCloud, Zoho). The login is verified with your provider before it is saved
- **Send Email** — To, Cc and Bcc fields that accept many addresses (paste a whole list), subject, plain-text or HTML body, drag-and-drop attachments
- **History** — every email with its recipients, attachments, status and message; searchable and paginated
- **Dashboard** — emails sent, people reached, last 24 hours, failures and recent activity

**Under the hood**

- JWT bearer authentication, per-user data isolation
- Mailbox passwords are AES-encrypted at rest and never returned by the API
- Server-side allow-list of SMTP providers (users cannot point the server at arbitrary hosts)
- Duplicate recipients are removed across To / Cc / Bcc; Bcc never appears in message headers
- Automatic EF Core migrations, Swagger UI, configurable CORS
- Repository + Unit of Work data layer, services, thin controllers

## Tech stack

| Area           | Technology                                                   |
| -------------- | ------------------------------------------------------------ |
| API            | ASP.NET Core 8 Web API                                       |
| Data access    | Entity Framework Core 8, SQL Server                          |
| Authentication | JWT Bearer                                                   |
| Email          | MailKit / MimeKit over SMTP (STARTTLS)                       |
| API docs       | Swashbuckle (Swagger / OpenAPI)                              |
| Frontend       | React 19, TypeScript, Vite, React Router, Axios              |

## Project structure

```
EmailToAll
├── EmailToAll.sln
├── src/EmailToAll              ASP.NET Core API
│   ├── Controllers/            HTTP endpoints
│   ├── Service/                Business logic (auth, email setup, sending, history, SMTP providers)
│   ├── Data/                   Repository interfaces/implementations and Unit of Work
│   ├── Models/                 EF Core entities and DbContext
│   ├── Payload/                Request / response DTOs
│   └── Migrations/             EF Core migrations
└── frontend                    React + TypeScript app
    └── src/
        ├── api/                Axios client and typed API calls
        ├── components/         Layout, recipient chips, file drop, modal...
        ├── context/            Auth state
        └── pages/              Login, Signup, Dashboard, SendEmail, History, EmailSetup
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20.19+ or 22.12+](https://nodejs.org)
- SQL Server — the default connection string targets **LocalDB** (`(localdb)\MSSQLLocalDB`)
- An SMTP account for the app's own emails (OTP codes). For Gmail, create an [app password](https://support.google.com/accounts/answer/185833)

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

`EmailSettings` is the account the app uses to send **OTP and password-reset codes**. Users send their own emails from the mailbox they connect in the app. Outside the `Development` environment the app refuses to start while any of these still hold the `REPLACE_ME` placeholders from `appsettings.json`.

### 3. Run the API

```bash
dotnet run --project src/EmailToAll
```

The database and tables are created automatically on first start. The API listens on **http://localhost:5018** and Swagger is at **http://localhost:5018/swagger**.

### 4. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Open **http://localhost:5173**. The dev server proxies `/api` to the API, so no CORS setup is needed. Point it at a different API with `VITE_API_PROXY` (see `frontend/.env.example`).

For a production build run `npm run build`; the output is in `frontend/dist`. Serve it from any static host, and set `VITE_API_URL` at build time if the API lives on another origin (then add that origin to `Cors:AllowedOrigins`).

## Using the app

1. **Sign up** and enter the 6-digit code that is emailed to you, then **sign in**.
2. Open **Email Setup**, enter the address and password (app password for Gmail, Yahoo and iCloud) you want to send from, and save.
3. Open **Send Email**, add recipients, write the message, attach files and send.
4. Follow everything in **History** and the **Dashboard**.

Limits per email: 500 recipients, 10 attachments, 25 MB of attachments in total.

## Configuration

| Key                                 | Description                                                                    |
| ----------------------------------- | ------------------------------------------------------------------------------ |
| `ConnectionStrings:Conn`            | SQL Server connection string                                                   |
| `EmailSettings:SmtpServer` / `Port` | SMTP host and port the app uses for OTP emails                                 |
| `EmailSettings:SenderName`          | Display name on OTP emails                                                     |
| `EmailSettings:SenderEmail`         | SMTP username and "from" address for OTP emails                                |
| `EmailSettings:Password`            | SMTP password / app password for OTP emails                                    |
| `EmailSettings:UseStartTls`         | `true` by default. Set `false` only for a local test SMTP server               |
| `JwtSettings:SecretKey`             | HMAC-SHA256 signing key (32+ characters)                                       |
| `JwtSettings:Issuer` / `Audience`   | Token issuer and audience                                                      |
| `JwtSettings:ExpiryMinutes`         | Token lifetime in minutes                                                      |
| `Encryption:Key` / `IV`             | AES key (32 chars) and IV (16 chars) used for stored passwords                 |
| `Cors:AllowedOrigins`               | Origins allowed by the `AllowFrontend` CORS policy                             |
| `SmtpProviders`                     | Mail providers users can connect: `Name`, `Domains`, `Host`, `Port`, `UseStartTls` |

Nested keys map to environment variables with a double underscore, e.g. `JwtSettings__SecretKey`. To support another provider, add an entry to `SmtpProviders`.

## API reference

All endpoints accept and return JSON (except sending, which is `multipart/form-data`). Responses follow the shape `{ "success": bool, "message": string?, "data": any? }`. Endpoints marked 🔒 need an `Authorization: Bearer <token>` header and answer `401` without one.

### Authentication

| Method | Endpoint                          | Body                                | Description                                      |
| ------ | --------------------------------- | ----------------------------------- | ------------------------------------------------ |
| POST   | `/api/Signup/Signup`              | `Email`, `Password`, `ConfirmPass`  | Register and email a verification OTP            |
| POST   | `/api/Signup/OptVerification`     | `Email`, `Token`                    | Verify the signup OTP                            |
| POST   | `/api/Signup/resendOtp`           | `Email`                             | Invalidate the previous OTP and send a new one   |
| POST   | `/api/Login/login`                | `Email`, `Password`                 | Sign in and receive a JWT                        |
| POST   | `/api/Login/ForgetPassword`       | `Email`                             | Email a password-reset OTP                       |
| POST   | `/api/Login/LoginOptVerification` | `Email`, `Token`                    | Check the reset OTP (it is consumed by the reset) |
| POST   | `/api/Login/PasswordReset`        | `Email`, `Token`, `Password`        | Set a new password using a valid OTP             |

### Email

| Method | Endpoint                | Description                                                                                                         |
| ------ | ----------------------- | ------------------------------------------------------------------------------------------------------------------- |
| GET 🔒  | `/api/EmailSetup`       | The connected mailbox (never includes the password)                                                                 |
| POST 🔒 | `/api/EmailSetup/Save`  | Body `Email`, `Password`. Verifies the login with the provider, then stores it encrypted                            |
| POST 🔒 | `/api/Email/Send`       | Form fields `To`, `Cc`, `Bcc` (repeatable), `Subject`, `Body`, `IsHtml`, `Attachments` (repeatable files)            |
| GET 🔒  | `/api/History`          | Query `search`, `page`, `pageSize`. Newest first                                                                    |
| GET 🔒  | `/api/History/{id}`     | One email including its body                                                                                        |
| GET 🔒  | `/api/Dashboard`        | Totals and recent activity                                                                                          |

## Contributing

Issues and pull requests are welcome. Please open an issue first to discuss larger changes.

## License

Released under the [MIT License](LICENSE).
