# RoadRules

A small Blazor Web App built as a practice project: a weight calculator that
returns the maximum weight for an axle group in a jurisdiction. The numbers
are invented and nothing here is real regulatory data.

## Pages

- **Overview** (`/`): counts from the database and links to the other pages
- **Jurisdictions** (`/jurisdictions`): each jurisdiction and how many limits it defines
- **Limit history** (`/history/{Code}`): every limit on record for one jurisdiction, with its status today
- **Compare** (`/compare`): the limit in effect per axle group for every jurisdiction, on any date
- **Calculator** (`/calculator`): the maximum for an axle group in a jurisdiction on a date

Limits are effective-dated: the calculator uses, per axle group, the latest limit whose
effective date is on or before the date asked.

## Stack

- Blazor Web App (.NET 8), Interactive Server render mode
- EF Core 8 with SQL Server (started on SQLite; see "Migration" below)
- xUnit tests against a fake data source
- Docker, multi-stage build

## Run

Start SQL Server in Docker:

```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=RoadRules_Pa55w0rd" -p 1433:1433 --name roadrules-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

Give it about 30 seconds (`docker logs roadrules-sql` should show "SQL Server is now ready"), then:

```
dotnet run --project RoadRules
```

Then open the URL printed. Migrations apply on startup.

## Test

```
dotnet test
```

The 10 tests use `FakeLimitSource`, so they need no database. They cover the axle-group lookup, the error cases, and picking the limit in effect on a date.

## Container

```
docker build -t roadrules .
docker run -p 8080:8080 -e "ConnectionStrings__Default=Server=host.docker.internal,1433;Database=RoadRules;User Id=sa;Password=RoadRules_Pa55w0rd;TrustServerCertificate=True" roadrules
```

Open http://localhost:8080. The connection string comes in as an environment
variable (`ConnectionStrings__Default` maps to `ConnectionStrings:Default`), so it is
not baked into the image.

## Migration

The project started on SQLite and was moved to SQL Server. Migrations were
regenerated for the new provider; ten rows that existed only in SQLite were
moved with an INSERT script (`extra_rows.sql`: IDENTITY_INSERT, an explicit column
list, ISO dates) and reconciled by row count and sum of MaxKg
(25 rows, 416,350 kg).

## Note

The SA password is in appsettings.json because this is a throwaway project.
In anything real it would come from user secrets locally and Key Vault in Azure.
