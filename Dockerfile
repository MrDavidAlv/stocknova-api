# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (layer caching)
COPY StockNova.sln .
COPY Directory.Build.props .
COPY src/StockNova.Domain/StockNova.Domain.csproj src/StockNova.Domain/
COPY src/StockNova.Application/StockNova.Application.csproj src/StockNova.Application/
COPY src/StockNova.Infrastructure/StockNova.Infrastructure.csproj src/StockNova.Infrastructure/
COPY src/StockNova.API/StockNova.API.csproj src/StockNova.API/
COPY tests/StockNova.UnitTests/StockNova.UnitTests.csproj tests/StockNova.UnitTests/
COPY tests/StockNova.IntegrationTests/StockNova.IntegrationTests.csproj tests/StockNova.IntegrationTests/
RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish src/StockNova.API/StockNova.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root
RUN adduser --disabled-password --gecos "" appuser

COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "StockNova.API.dll"]
