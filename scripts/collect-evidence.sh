#!/usr/bin/env bash
# =============================================================================
# collect-evidence.sh - coleta evidências de um ambiente em execução
#   (containers, imagens, redes, volumes, logs e respostas da API) na pasta
#   evidence/<ambiente>/. O pipeline publica essa pasta como artifact.
#
# Uso: scripts/collect-evidence.sh <staging|production>
# =============================================================================
set -uo pipefail

ENVIRONMENT="${1:?informe o ambiente: staging ou production}"
case "$ENVIRONMENT" in
  staging)    COMPOSE_FILE="docker-compose.staging.yml";    DEFAULT_PORT=8081 ;;
  production) COMPOSE_FILE="docker-compose.production.yml"; DEFAULT_PORT=8082 ;;
  *) echo "ambiente inválido"; exit 2 ;;
esac

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"
ENV_FILE=".env.$ENVIRONMENT"
[[ -f "$ENV_FILE" ]] && { set -a; source "$ENV_FILE"; set +a; }
API_PORT="${API_PORT:-$DEFAULT_PORT}"
OUT="evidence/$ENVIRONMENT"
mkdir -p "$OUT"

COMPOSE=(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE")

{
  echo "# Evidências do ambiente: $ENVIRONMENT"
  echo "Gerado em: $(date -u +'%Y-%m-%dT%H:%M:%SZ')"
  echo; echo "## docker compose ps"; "${COMPOSE[@]}" ps
  echo; echo "## docker images (api)"; docker images --format 'table {{.Repository}}\t{{.Tag}}\t{{.ID}}\t{{.Size}}\t{{.CreatedSince}}' | grep -E 'REPOSITORY|compliance' || true
  echo; echo "## docker network ls"; docker network ls | grep -E 'NETWORK|compliance' || true
  echo; echo "## docker volume ls"; docker volume ls | grep -E 'VOLUME|compliance' || true
  echo; echo "## labels da imagem da API"
  docker inspect --format '{{json .Config.Labels}}' "$("${COMPOSE[@]}" ps -q api)" 2>/dev/null || true
  echo; echo "## GET /api/info"; curl -s "http://localhost:$API_PORT/api/info"; echo
  echo; echo "## GET /health/ready"; curl -s -w ' (HTTP %{http_code})' "http://localhost:$API_PORT/health/ready"; echo
} > "$OUT/ambiente.md" 2>&1

"${COMPOSE[@]}" logs --no-color --timestamps api > "$OUT/api.log" 2>&1
"${COMPOSE[@]}" logs --no-color --timestamps --tail 200 db > "$OUT/db.log" 2>&1
curl -s "http://localhost:$API_PORT/api/info" > "$OUT/api-info.json" || true
curl -s "http://localhost:$API_PORT/swagger/v1/swagger.json" > "$OUT/swagger.json" || true

echo "Evidências gravadas em $OUT/"
ls -la "$OUT"
