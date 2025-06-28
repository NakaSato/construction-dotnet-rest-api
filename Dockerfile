# Use the official .NET 9.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["dotnet-rest-api.csproj", "."]
RUN dotnet restore "dotnet-rest-api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "dotnet-rest-api.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "dotnet-rest-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Create uploads directory
RUN mkdir -p uploads

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_HTTP_PORTS=8080;443

ENTRYPOINT ["dotnet", "dotnet-rest-api.dll"]
