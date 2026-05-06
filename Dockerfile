FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/JobService.Api/JobService.Api.csproj", "src/JobService.Api/"]
COPY ["src/JobService.Core/JobService.Core.csproj", "src/JobService.Core/"]
COPY ["src/JobService.Infrastructure/JobService.Infrastructure.csproj", "src/JobService.Infrastructure/"]
RUN dotnet restore "src/JobService.Api/JobService.Api.csproj"

COPY . .
RUN dotnet publish "src/JobService.Api/JobService.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JobService.Api.dll"]
