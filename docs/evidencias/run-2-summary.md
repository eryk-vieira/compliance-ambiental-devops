# Execução #2 do pipeline — commit 47a7b75 (main)
URL: https://github.com/eryk-vieira/compliance-ambiental-devops/actions/runs/38490906947
Status: Success | Duração total: 10m 30s | Artefatos: 5

| Job | Duração | Resultado |
|---|---|---|
| Build (.NET 8) | 36s | ✅ versão 1.0.2+47a7b75, 61 arquivos publicados |
| Testes automatizados (xUnit) | 33s | ✅ 15 testes, 15 aprovados, 0 falhas |
| Imagem Docker (GHCR) | 3m 54s | ✅ ghcr.io/eryk-vieira/compliance-ambiental-api:{main, sha-47a7b75, build-2} |
| Deploy STAGING | 1m 9s | ✅ porta 8081, smoke test 12/12 OK, imagem promovida para :staging |
| Aprovação | — | eryk-vieira approved production — "Aprovado" |
| Deploy PRODUÇÃO | 58s | ✅ porta 8082, smoke test 12/12 OK, imagem promovida para :production e :latest |

## /api/info — STAGING
{"aplicacao":"Compliance Ambiental Logtech API","ambiente":"staging","aspnetEnvironment":"Staging","versao":"1.0.2+47a7b75","commit":"47a7b75894093eaf652a5a2f44d42d52409de66e","build":"2","deployEm":"2026-10-14T00:39:20Z","horaServidorUtc":"2026-10-14T00:40:09.4545109Z","host":"1fd622af0552"}

## /api/info — PRODUÇÃO
{"aplicacao":"Compliance Ambiental Logtech API","ambiente":"production","aspnetEnvironment":"Production","versao":"1.0.2+47a7b75","commit":"47a7b75894093eaf652a5a2f44d42d52409de66e","build":"2","deployEm":"2026-10-14T00:43:35Z","horaServidorUtc":"2026-10-14T00:44:15.5525163Z","host":"82c05be2018b"}

## docker compose ps — STAGING
compliance-staging-api  ghcr.io/eryk-vieira/compliance-ambiental-api:sha-47a7b75  Up (healthy)  0.0.0.0:8081->8080/tcp
compliance-staging-db   mcr.microsoft.com/mssql/server:2022-latest               Up (healthy)  0.0.0.0:14331->1433/tcp

## docker compose ps — PRODUÇÃO
compliance-prod-api  ghcr.io/eryk-vieira/compliance-ambiental-api:sha-47a7b75  Up (healthy)  0.0.0.0:8082->8080/tcp
compliance-prod-db   mcr.microsoft.com/mssql/server:2022-latest               Up (healthy)  1433/tcp (não exposto)

## Smoke test — STAGING (http://localhost:8081)
[1/5] Aguardando /health/ready ...
[OK] API pronta após 1 tentativa(s): Healthy
[2/5] Verificando /health/live e /api/info ...
[OK] /health/live -> 200
[OK] ambiente reportado = staging
[3/5] Verificando que endpoints protegidos exigem JWT ...
[OK] GET /api/transportadoras sem token -> 401
[4/5] Autenticando (admin) ...
[OK] token JWT obtido (484 chars)
[5/5] Chamando endpoints de negócio autenticados ...
[OK] GET /api/transportadoras?pageNumber=1&pageSize=5 -> 200 (684 bytes)
[OK] GET /api/operacoes?pageNumber=1&pageSize=5 -> 200 (1403 bytes)
[OK] GET /api/emissoes/consolidado?pageNumber=1&pageSize=5 -> 200 (815 bytes)
[OK] GET /api/emissoes/alertas -> 200 (107 bytes)
[OK] GET /api/licencas/status?pageNumber=1&pageSize=5 -> 200 (816 bytes)
[OK] GET /api/transportadoras/1/dashboard -> 200 (641 bytes)
[OK] GET /swagger/v1/swagger.json -> 200 (28109 bytes)
RESUMO: 12 verificações OK, 0 falha(s)
RESULTADO: SUCESSO

## Smoke test — PRODUÇÃO (http://localhost:8082)
[1/5] Aguardando /health/ready ...
[OK] API pronta após 1 tentativa(s): Healthy
[2/5] Verificando /health/live e /api/info ...
[OK] /health/live -> 200
[OK] ambiente reportado = production
[3/5] Verificando que endpoints protegidos exigem JWT ...
[OK] GET /api/transportadoras sem token -> 401
[4/5] Autenticando (admin) ...
[OK] token JWT obtido (484 chars)
[5/5] Chamando endpoints de negócio autenticados ...
[OK] GET /api/transportadoras?pageNumber=1&pageSize=5 -> 200 (684 bytes)
[OK] GET /api/operacoes?pageNumber=1&pageSize=5 -> 200 (1403 bytes)
[OK] GET /api/emissoes/consolidado?pageNumber=1&pageSize=5 -> 200 (815 bytes)
[OK] GET /api/emissoes/alertas -> 200 (107 bytes)
[OK] GET /api/licencas/status?pageNumber=1&pageSize=5 -> 200 (816 bytes)
[OK] GET /api/transportadoras/1/dashboard -> 200 (641 bytes)
[OK] GET /swagger/v1/swagger.json -> 200 (28109 bytes)
RESUMO: 12 verificações OK, 0 falha(s)
RESULTADO: SUCESSO
