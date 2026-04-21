FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/JobService.Api/JobService.Api.csproj", "src/JobService.Api/"]
COPY ["src/JobService.Core/JobService.Core.csproj", "src/JobService.Core/"]
COPY ["src/JobService.Infrastructure/JobService.Infrastructure.csproj", "src/JobService.Infrastructure/"]
RUN dotnet restore "src/JobService.Api/JobService.Api.csproj"
COPY . .
RUN dotnet build "src/JobService.Api/JobService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/JobService.Api/JobService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobService.Api.dll"]
