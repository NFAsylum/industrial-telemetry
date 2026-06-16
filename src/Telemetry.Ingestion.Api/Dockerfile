FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Telemetry.Contracts/Telemetry.Contracts.csproj src/Telemetry.Contracts/
COPY src/Telemetry.Domain/Telemetry.Domain.csproj src/Telemetry.Domain/
COPY src/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj src/Telemetry.Infrastructure/
COPY src/Telemetry.Ingestion.Api/Telemetry.Ingestion.Api.csproj src/Telemetry.Ingestion.Api/

RUN dotnet restore src/Telemetry.Ingestion.Api/Telemetry.Ingestion.Api.csproj

COPY src/ src/
RUN dotnet publish src/Telemetry.Ingestion.Api/Telemetry.Ingestion.Api.csproj \
  -c Release \
  -o /publish \
  /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER app

ENTRYPOINT ["dotnet", "Telemetry.Ingestion.Api.dll"]
