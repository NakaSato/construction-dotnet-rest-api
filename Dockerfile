# Use the official .NET 10.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link dotnet-rest-api.csproj .
RUN dotnet restore

# Copy source code and publish app
COPY --link . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 8080
WORKDIR /app
COPY --link --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_URLS=http://+:8080
# Hosts with a low inotify instance limit (e.g. Render free tier, limit 128) crash
# in WebApplication.CreateBuilder when the config provider starts a FileSystemWatcher
# on appsettings*.json. Polling avoids inotify entirely.
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
# No PostgreSQL is attached in the container-hosted environment, and the
# Docker connection string targets the compose-only `postgres-dev` host.
# Override with CONNECTIONSTRINGS__DEFAULT (and unset this) to use a real DB.
ENV USE_IN_MEMORY_DB=true
ENTRYPOINT ["dotnet", "dotnet-rest-api.dll"]
