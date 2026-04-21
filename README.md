# job-tracker-app-job-service

Manages job requisition records including creation, status tracking, and job description storage.

## Technology
- .NET 8 Web API
- C#
- PostgreSQL
- Docker

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project src/JobService.Api
```

## Running with Docker

```bash
docker build -t job-tracker-app-job-service .
docker run -p 5001:5001 job-tracker-app-job-service
```

## Environment variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Auth0__Domain` | Auth0 domain |
| `Auth0__Audience` | Auth0 API audience |
| `Kafka__BootstrapServers` | Kafka broker address |

## Project structure

```
src/
  JobService.Api/          # Web API entry point, controllers, middleware
  JobService.Core/         # Domain models, interfaces, business logic
  JobService.Infrastructure/ # Data access, Kafka, external integrations
tests/
  JobService.UnitTests/
  JobService.IntegrationTests/
```
