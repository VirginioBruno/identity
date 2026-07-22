# Identity API

A compact identity service built with ASP.NET Core. It demonstrates credential validation, JWT authentication, role-based authorization, PostgreSQL persistence, and automated tests against a real database.

> This is a study and portfolio project. It is not intended to replace a managed identity provider or a security-reviewed authentication platform.

## What the project demonstrates

- Password hashing with BCrypt
- Short-lived, configurable JWT access tokens
- Role-based authorization for user administration
- API versioning
- Entity Framework Core with PostgreSQL migrations
- Unit tests and integration tests with Testcontainers
- Containerized local development and GitHub Actions CI

## Technology

- .NET 10 and ASP.NET Core
- Entity Framework Core 10 and PostgreSQL
- xUnit, NSubstitute, and Testcontainers
- Docker and Docker Compose

## API

| Method | Route | Authorization | Purpose |
|---|---|---|---|
| `POST` | `/api/v1/authentication` | Anonymous | Validate credentials and issue a JWT |
| `POST` | `/api/v1/users` | Admin | Create a user with the `admin` or `user` role |
| `PATCH` | `/api/v1/users/{userId}/activate` | Admin | Activate a user |
| `PATCH` | `/api/v1/users/{userId}/inactivate` | Admin | Inactivate a user |

Authentication deliberately returns the same `401 Unauthorized` response for an unknown user and an invalid password, which avoids exposing whether a username exists.

## Run with Docker

Copy the example environment file and replace every placeholder with a local value:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

The API is available at `http://localhost:8080`. The `admin` and `user` roles are created automatically. On first startup, a bootstrap administrator is created only when `BOOTSTRAP_ADMIN_PASSWORD` is configured. Clear that value after the account has been created if you do not need bootstrap behavior again.

The `.env` file is ignored by Git and must never be committed.

## Run from the .NET CLI

Start PostgreSQL and provide configuration through environment variables or .NET user secrets:

```powershell
dotnet user-secrets init --project src/identity.api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=identity;Username=identity;Password=your-password" --project src/identity.api
dotnet user-secrets set "Jwt:SigningKey" "use-at-least-32-random-characters" --project src/identity.api
dotnet run --project src/identity.api
```

JWT issuer, audience, and token lifetime have non-secret defaults in `appsettings.json`. The signing key does not.

## Tests

Unit tests do not require external infrastructure:

```powershell
dotnet test tests/identity.unit.tests
```

Integration tests require Docker because Testcontainers starts an isolated PostgreSQL container on a random host port:

```powershell
dotnet test tests/identity.integration.tests
```

The CI workflow builds the complete solution and executes both test projects on every push and pull request targeting `main`.

## Security decisions and limitations

- JWT signing material and database credentials are supplied at runtime and are not stored in source control.
- User password hashes are excluded from JSON responses.
- User-management routes require the `admin` role.
- CORS uses an explicit origin allowlist instead of accepting every origin.
- Access tokens are intentionally short-lived, but refresh tokens and token revocation are not implemented.
- Account lockout, password recovery, email verification, MFA, key rotation, and audit trails are outside the current scope.
- Database migrations currently run during application startup. This is convenient for the portfolio deployment model, but a production environment would normally run migrations as a separate, controlled deployment step.
