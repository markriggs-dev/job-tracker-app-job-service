# job-tracker-app-job-service

Manages job requisition records including creation, editing, status tracking, and job description storage.

## Technology
- .NET 8 Web API
- C#
- PostgreSQL via Entity Framework Core
- Apache Kafka (Confluent.Kafka producer)
- Docker

## Kafka Events Published

| Topic | Trigger | Payload |
|-------|---------|---------|
| `job.application.created` | POST /api/jobs | JobReqId, UserId, UserEmail, CompanyName, RoleTitle, JobDescription, OccurredAt |
| `job.application.updated` | PUT /api/jobs/{id} | JobReqId, UserId, UserEmail, CompanyName, RoleTitle, JobDescription, OccurredAt |

Status changes (PATCH /api/jobs/{id}/status) are synchronous — no Kafka event is published.

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project src/JobService.Api
```

## Running with Docker

```bash
docker build -t job-tracker-app-job-service .
docker run -p 5153:8080 job-tracker-app-job-service
```

## Environment variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Auth0__Domain` | Auth0 domain |
| `Auth0__Audience` | Auth0 API audience |
| `Kafka__BootstrapServers` | Kafka broker address (default: localhost:9092) |

## Project structure

```
src/
  JobService.Api/            # Web API entry point, controllers, middleware
  JobService.Core/           # Domain models, interfaces, business logic
  JobService.Infrastructure/ # Data access, Kafka producer
tests/
  JobService.UnitTests/
  JobService.IntegrationTests/
```
