FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Telemetry.Contracts/Telemetry.Contracts.csproj src/Telemetry.Contracts/
COPY src/Telemetry.Domain/Telemetry.Domain.csproj src/Telemetry.Domain/
COPY src/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj src/Telemetry.Infrastructure/
COPY src/Telemetry.Worker/Telemetry.Worker.csproj src/Telemetry.Worker/

RUN dotnet restore src/Telemetry.Worker/Telemetry.Worker.csproj

COPY src/ src/
RUN dotnet publish src/Telemetry.Worker/Telemetry.Worker.csproj \
  -c Release \
  -o /publish \
  /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish ./

USER app

ENTRYPOINT ["dotnet", "Telemetry.Worker.dll"]
