# syntax=docker/dockerfile:1
# Multi-stage build for OrderService.Api.
# Build context MUST be the `backend/` directory, e.g.:
#   docker build -f services/order-service/Dockerfile -t order-service:local .
# (run from inside backend/) so the shared ServiceDefaults/BuildingBlocks
# project references resolve correctly.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/order-service/OrderService.Api/OrderService.Api.csproj", "services/order-service/OrderService.Api/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["shared/BuildingBlocks/BuildingBlocks.csproj", "shared/BuildingBlocks/"]
RUN dotnet restore "services/order-service/OrderService.Api/OrderService.Api.csproj"

COPY . .
WORKDIR /src/services/order-service/OrderService.Api
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
