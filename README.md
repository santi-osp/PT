# DataGo

DataGo is a technical-exercise monorepo for managing residential customers. The backend supports creation, read, search, update, and logical retirement while keeping business rules independent from HTTP and PostgreSQL.

## Stack and structure

- ASP.NET Core 10, C# 14, Swagger/OpenAPI
- EF Core 10.0.8, Npgsql 10.0.3, and PostgreSQL stored procedures/functions
- PostgreSQL (Neon for the shared development database)
- xUnit focused domain/application tests

```text
backend/
  src/DataGo.Domain
  src/DataGo.Application
  src/DataGo.Infrastructure
  src/DataGo.Api
  tests/
docs/
frontend/                 # Goal 3
```

See [architecture](docs/architecture.md), [database model](docs/database.md), and [decision log](docs/decisions.md).

Data persistence is executed through version-controlled PostgreSQL stored routines while business rules remain in Domain/Application.

## Local configuration

The API reads `ConnectionStrings:DefaultConnection` through `IConfiguration`. The API project already has `UserSecretsId`; from the repository root, store a local PostgreSQL/Neon connection string with:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_POSTGRESQL_CONNECTION_STRING>" --project .\backend\src\DataGo.Api\DataGo.Api.csproj
```

Npgsql uses keyword/value connection strings. In Neon, select its **.NET** connection-string format. If only a PostgreSQL URI is available, map its host, port, database, user, and password to:

```text
Host=<host>;Port=<port>;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require
```

Keep the password only in user-secrets. A later production deployment will set `ConnectionStrings__DefaultConnection` as an environment variable.

## Build, migrate, and run

From the repository root:

```powershell
dotnet restore .\backend\DataGo.sln
dotnet build .\backend\DataGo.sln --no-restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project .\backend\src\DataGo.Infrastructure\DataGo.Infrastructure.csproj --startup-project .\backend\src\DataGo.Api\DataGo.Api.csproj
dotnet run --project .\backend\src\DataGo.Api\DataGo.Api.csproj
```

Swagger UI is available at `/swagger`; health is available at `/api/health`.

## Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/residential-customers` | Create |
| GET | `/api/residential-customers?search=&status=all` | Search/list; status accepts `all`, `active`, `blocked` |
| GET | `/api/residential-customers/{id}` | Detail |
| PUT | `/api/residential-customers/{id}` | Update editable fields |
| PATCH | `/api/residential-customers/{id}/retire` | Logical retirement |
| GET | `/api/catalogs/neighborhoods?query=` | Neighborhood catalog |
| GET | `/api/catalogs/centers` | Active center catalog |
| GET | `/api/health` | Process health |

## Assumptions and demonstration data

Retired customers remain visible and include `warning: "Cliente bloqueado"`. A retired customer cannot be updated or retired again. `BusinessName` is independent from the legal name. Company-name update behavior is documented in the decision log.

`InitialCreate` seeds Colombia/Antioquia/Medellín, eight transport zones, 44 demonstration neighborhoods, 20 fictitious centers, and five synthetic modern-channel documents. These are migration-controlled demonstration records, not official commercial or cartographic data.
