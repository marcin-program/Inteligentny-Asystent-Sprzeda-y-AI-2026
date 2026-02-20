# Multi-stage Dockerfile for .NET 8.0 application
# Stage 1: Build
# Stage 2: Runtime (smaller image)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY . .

# Restore dependencies (cached layer if no .csproj changes)
RUN dotnet restore src/PetWorld.WebUI/PetWorld.WebUI.csproj

# Build and publish
RUN dotnet publish src/PetWorld.WebUI/PetWorld.WebUI.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Run application
ENTRYPOINT ["dotnet", "PetWorld.WebUI.dll"]