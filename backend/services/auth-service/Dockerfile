# syntax=docker/dockerfile:1
# Multi-stage build for AuthService.Api.
# Build context MUST be the `backend/` directory, e.g.:
#   docker build -f services/auth-service/Dockerfile -t auth-service:local .
# (run from inside backend/) so the shared ServiceDefaults/BuildingBlocks
# project references resolve correctly.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/auth-service/AuthService.Api/AuthService.Api.csproj", "services/auth-service/AuthService.Api/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["shared/BuildingBlocks/BuildingBlocks.csproj", "shared/BuildingBlocks/"]
RUN dotnet restore "services/auth-service/AuthService.Api/AuthService.Api.csproj"

COPY . .
WORKDIR /src/services/auth-service/AuthService.Api
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
