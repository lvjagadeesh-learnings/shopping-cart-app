# syntax=docker/dockerfile:1
# Multi-stage build for CartService.Api.
# Build context MUST be the `backend/` directory, e.g.:
#   docker build -f services/cart-service/Dockerfile -t cart-service:local .
# (run from inside backend/) so the shared ServiceDefaults/BuildingBlocks
# project references resolve correctly.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/cart-service/CartService.Api/CartService.Api.csproj", "services/cart-service/CartService.Api/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["shared/BuildingBlocks/BuildingBlocks.csproj", "shared/BuildingBlocks/"]
RUN dotnet restore "services/cart-service/CartService.Api/CartService.Api.csproj"

COPY . .
WORKDIR /src/services/cart-service/CartService.Api
RUN dotnet publish "CartService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CartService.Api.dll"]
