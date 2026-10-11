#!/usr/bin/env bash
# =============================================================================
# smoke-test.sh - valida um ambiente implantado (staging ou produção)
#
# Uso: scripts/smoke-test.sh <BASE_URL> [AMBIENTE_ESPERADO]
#   ex: scripts/smoke-test.sh http://localhost:8081 staging
#
# Passos:
#   1) aguarda /health/ready responder 200 (até ~3 min)
#   2) confere /api/info (ambiente esperado)
#   3) autentica em /api/auth/login e chama endpoints protegidos
#   4) confere que sem token a API devolve 401
# Sai com código != 0 se qualquer verificação falhar (quebra o pipeline).
# =============================================================================
set -euo pipefail

BASE_URL="${1:?informe a BASE_URL, ex: http://localhost:8081}"
EXPECTED_ENV="${2:-}"

PASS=0; FAIL=0
ok()   { echo "  [OK]   $*"; PASS=$((PASS+1)); }
fail() { echo "  [FAIL] $*"; FAIL=$((FAIL+1)); }

echo "================================================================="
echo " SMOKE TEST - Compliance Ambiental API"
echo " URL:      $BASE_URL"
echo " Ambiente: ${EXPECTED_ENV:-<não verificado>}"
echo " Data:     $(date -u +'%Y-%m-%dT%H:%M:%SZ')"
echo "================================================================="

# 1) Readiness ---------------------------------------------------------------
echo "[1/5] Aguardando /health/ready ..."
for i in $(seq 1 36); do
  if curl -fsS "$BASE_URL/health/ready" >/dev/null 2>&1; then
    ok "API pronta após ${i} tentativa(s): $(curl -fsS "$BASE_URL/health/ready")"
    break
  fi
  if [[ $i -eq 36 ]]; then fail "API não ficou pronta em 3 minutos"; echo "RESULTADO: FALHOU"; exit 1; fi
  sleep 5
done

# 2) Liveness + info ---------------------------------------------------------
echo "[2/5] Verificando /health/live e /api/info ..."
LIVE=$(curl -s -o /dev/null -w '%{http_code}' "$BASE_URL/health/live")
[[ "$LIVE" == "200" ]] && ok "/health/live -> 200" || fail "/health/live -> $LIVE"

INFO=$(curl -fsS "$BASE_URL/api/info")
echo "  /api/info: $INFO"
if [[ -n "$EXPECTED_ENV" ]]; then
  if echo "$INFO" | grep -q "\"ambiente\":\"$EXPECTED_ENV\""; then
    ok "ambiente reportado = $EXPECTED_ENV"
  else
    fail "ambiente reportado diferente de $EXPECTED_ENV"
  fi
fi

# 3) Sem token -> 401 --------------------------------------------------------
echo "[3/5] Verificando que endpoints protegidos exigem JWT ..."
CODE=$(curl -s -o /dev/null -w '%{http_code}' "$BASE_URL/api/transportadoras")
[[ "$CODE" == "401" ]] && ok "GET /api/transportadoras sem token -> 401" || fail "GET /api/transportadoras sem token -> $CODE (esperado 401)"

# 4) Login -------------------------------------------------------------------
echo "[4/5] Autenticando (admin) ..."
LOGIN_JSON=$(curl -fsS -X POST "$BASE_URL/api/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"login":"admin","senha":"Admin@123"}')
TOKEN=$(echo "$LOGIN_JSON" | sed -n 's/.*"accessToken":"\([^"]*\)".*/\1/p')
if [[ -n "$TOKEN" ]]; then ok "token JWT obtido (${#TOKEN} chars)"; else fail "login não retornou accessToken: $LOGIN_JSON"; fi

# 5) Endpoints de negócio ----------------------------------------------------
echo "[5/5] Chamando endpoints de negócio autenticados ..."
for path in \
  "/api/transportadoras?pageNumber=1&pageSize=5" \
  "/api/operacoes?pageNumber=1&pageSize=5" \
  "/api/emissoes/consolidado?pageNumber=1&pageSize=5" \
  "/api/emissoes/alertas" \
  "/api/licencas/status?pageNumber=1&pageSize=5" \
  "/api/transportadoras/1/dashboard" \
  "/swagger/v1/swagger.json"
do
  CODE=$(curl -s -o /tmp/smoke_body.json -w '%{http_code}' -H "Authorization: Bearer $TOKEN" "$BASE_URL$path")
  if [[ "$CODE" == "200" ]]; then
    ok "GET $path -> 200 ($(wc -c </tmp/smoke_body.json) bytes)"
  else
    fail "GET $path -> $CODE"
  fi
done

echo "-----------------------------------------------------------------"
echo " Amostra: GET /api/emissoes/alertas"
curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/emissoes/alertas" | head -c 800; echo
echo "-----------------------------------------------------------------"
echo " RESUMO: $PASS verificações OK, $FAIL falha(s)"
if [[ $FAIL -gt 0 ]]; then echo " RESULTADO: FALHOU"; exit 1; fi
echo " RESULTADO: SUCESSO"
