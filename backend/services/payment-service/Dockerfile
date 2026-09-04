# syntax=docker/dockerfile:1
# Multi-stage build for PaymentService.Api.
# Build context MUST be the `backend/` directory, e.g.:
#   docker build -f services/payment-service/Dockerfile -t payment-service:local .
# (run from inside backend/) so the shared ServiceDefaults/BuildingBlocks
# project references resolve correctly.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["services/payment-service/PaymentService.Api/PaymentService.Api.csproj", "services/payment-service/PaymentService.Api/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["shared/BuildingBlocks/BuildingBlocks.csproj", "shared/BuildingBlocks/"]
RUN dotnet restore "services/payment-service/PaymentService.Api/PaymentService.Api.csproj"

COPY . .
WORKDIR /src/services/payment-service/PaymentService.Api
RUN dotnet publish "PaymentService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
