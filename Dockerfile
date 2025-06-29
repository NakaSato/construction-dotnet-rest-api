# Use the official .NET 9.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link dotnet-rest-api.csproj .
RUN dotnet restore

# Copy source code and publish app
COPY --link . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 8080
WORKDIR /app
COPY --link --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "dotnet-rest-api.dll"]
