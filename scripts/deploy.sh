#!/usr/bin/env bash
# =============================================================================
# deploy.sh - sobe (ou atualiza) um ambiente com Docker Compose e valida com
#             smoke test. É o mesmo script usado pelos jobs de deploy do pipeline.
#
# Uso: scripts/deploy.sh <staging|production> [--build] [--down]
#   --build  força o build local da imagem (ignora API_IMAGE do GHCR)
#   --down   derruba o ambiente (mantém o volume do banco)
#
# Variáveis são lidas de .env.<ambiente> (crie a partir de .env.<ambiente>.example).
# =============================================================================
set -euo pipefail

ENVIRONMENT="${1:?informe o ambiente: staging ou production}"
shift || true
BUILD=false; DOWN=false
for arg in "$@"; do
  case "$arg" in
    --build) BUILD=true ;;
    --down)  DOWN=true ;;
    *) echo "argumento desconhecido: $arg"; exit 2 ;;
  esac
done

case "$ENVIRONMENT" in
  staging)    COMPOSE_FILE="docker-compose.staging.yml";    DEFAULT_PORT=8081 ;;
  production) COMPOSE_FILE="docker-compose.production.yml"; DEFAULT_PORT=8082 ;;
  *) echo "ambiente inválido: $ENVIRONMENT (use staging ou production)"; exit 2 ;;
esac

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

ENV_FILE=".env.$ENVIRONMENT"
if [[ ! -f "$ENV_FILE" ]]; then
  echo ">> $ENV_FILE não encontrado; criando a partir de $ENV_FILE.example"
  cp "$ENV_FILE.example" "$ENV_FILE"
fi

# Carrega as variáveis para este shell (e as exporta para o compose)
set -a; # shellcheck disable=SC1090
source "$ENV_FILE"; set +a
export APP_DEPLOYED_AT="${APP_DEPLOYED_AT:-$(date -u +'%Y-%m-%dT%H:%M:%SZ')}"
API_PORT="${API_PORT:-$DEFAULT_PORT}"

COMPOSE=(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE")

if $DOWN; then
  echo ">> Derrubando ambiente $ENVIRONMENT ..."
  "${COMPOSE[@]}" down --remove-orphans
  exit 0
fi

echo "================================================================="
echo " DEPLOY -> $ENVIRONMENT"
echo " compose:  $COMPOSE_FILE"
echo " imagem:   ${API_IMAGE:-<build local>}"
echo " versão:   ${APP_VERSION:-dev}  commit: ${APP_COMMIT_SHA:-local}"
echo " porta:    $API_PORT"
echo "================================================================="

if $BUILD || [[ -z "${API_IMAGE:-}" ]]; then
  echo ">> Build da imagem local ..."
  "${COMPOSE[@]}" build --pull api
  UP_ARGS=(--no-build)
else
  echo ">> Baixando imagem $API_IMAGE ..."
  docker pull "$API_IMAGE"
  UP_ARGS=(--no-build)
fi

echo ">> Subindo serviços (aguarda healthchecks) ..."
"${COMPOSE[@]}" up -d --wait --wait-timeout 300 --remove-orphans "${UP_ARGS[@]}"

echo ">> Estado dos containers:"
"${COMPOSE[@]}" ps

echo ">> Smoke test ..."
mkdir -p "evidence/$ENVIRONMENT"
bash scripts/smoke-test.sh "http://localhost:$API_PORT" "$ENVIRONMENT" 2>&1 | tee "evidence/$ENVIRONMENT/smoke-test.txt"

echo ">> Deploy em $ENVIRONMENT concluído com sucesso."
