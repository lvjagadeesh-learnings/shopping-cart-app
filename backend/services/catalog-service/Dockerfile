# syntax=docker/dockerfile:1
# Multi-stage build for CatalogService.Api.
# Build context MUST be the `backend/` directory, e.g.:
#   docker build -f services/catalog-service/Dockerfile -t catalog-service:local .
# (run from inside backend/) so the shared ServiceDefaults/BuildingBlocks
# project references resolve correctly.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/catalog-service/CatalogService.Api/CatalogService.Api.csproj", "services/catalog-service/CatalogService.Api/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["shared/BuildingBlocks/BuildingBlocks.csproj", "shared/BuildingBlocks/"]
RUN dotnet restore "services/catalog-service/CatalogService.Api/CatalogService.Api.csproj"

COPY . .
WORKDIR /src/services/catalog-service/CatalogService.Api
RUN dotnet publish "CatalogService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]
