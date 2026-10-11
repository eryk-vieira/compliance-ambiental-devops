# =============================================================================
# Dockerfile - Compliance Ambiental Logtech API (.NET 8)
# Estratégia: build multi-stage
#   1) build   : SDK completo, restaura e publica em Release (camadas cacheáveis)
#   2) runtime : apenas ASP.NET Runtime (imagem ~4x menor), usuário não-root,
#                healthcheck embutido e metadados OCI preenchidos pelo pipeline.
# =============================================================================

# ---------------------------------------------------------------------------
# Estágio 1: build
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1a) Restaura dependências primeiro: só é reexecutado se os .csproj/.sln mudarem.
COPY ComplianceAmbiental.sln ./
COPY src/ComplianceAmbiental.Api/ComplianceAmbiental.Api.csproj src/ComplianceAmbiental.Api/
COPY tests/ComplianceAmbiental.Tests/ComplianceAmbiental.Tests.csproj tests/ComplianceAmbiental.Tests/
RUN dotnet restore ComplianceAmbiental.sln

# 1b) Copia o restante do código e publica a API.
COPY . .
RUN dotnet publish src/ComplianceAmbiental.Api/ComplianceAmbiental.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Estágio 2: runtime
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Metadados injetados pelo pipeline (docker build --build-arg ...)
ARG APP_VERSION=dev
ARG APP_COMMIT_SHA=local
ARG APP_BUILD_NUMBER=local
ARG BUILD_DATE=unknown

LABEL org.opencontainers.image.title="compliance-ambiental-api" \
      org.opencontainers.image.description="API RESTful de Governança e Compliance Ambiental (ESG / Logtech) - FIAP" \
      org.opencontainers.image.version="${APP_VERSION}" \
      org.opencontainers.image.revision="${APP_COMMIT_SHA}" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.authors="Eryk Vieira - RM 564801"

# curl é usado pelo HEALTHCHECK e pelos smoke tests dentro do container.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Configuração padrão (sobrescrita pelas variáveis do docker-compose / .env)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    APP_VERSION=${APP_VERSION} \
    APP_COMMIT_SHA=${APP_COMMIT_SHA} \
    APP_BUILD_NUMBER=${APP_BUILD_NUMBER} \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

# Usuário não-root fornecido pela imagem base (uid 1654)
USER app

HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=5 \
    CMD curl -fsS http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "ComplianceAmbiental.Api.dll"]
