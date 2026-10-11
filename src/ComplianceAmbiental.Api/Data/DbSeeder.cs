using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.Security;
using ComplianceAmbiental.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Data;

/// <summary>
/// Popula o banco com dados de exemplo coerentes para demonstrar todos os endpoints
/// (transportadoras, licenças, veículos, operações e acumulados mensais de CO2).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Usuarios.AnyAsync(ct))
            return; // banco já populado

        // ----- Usuários (autenticação) -----
        db.Usuarios.AddRange(
            new Usuario { Login = "admin", SenhaHash = PasswordHasher.Hash("Admin@123"), Perfil = "Admin" },
            new Usuario { Login = "auditor", SenhaHash = PasswordHasher.Hash("Auditor@123"), Perfil = "Auditor" }
        );

        var hoje = DateTime.UtcNow.Date;

        // ----- Transportadoras -----
        var tEcoLog = new Transportadora { RazaoSocial = "EcoLog Transportes Ltda", Cnpj = "12345678000190", Uf = "SP", LimiteMensalCo2Kg = 50000m };
        var tRodoVerde = new Transportadora { RazaoSocial = "RodoVerde Logística S/A", Cnpj = "98765432000110", Uf = "PR", LimiteMensalCo2Kg = 30000m };
        var tCargaSul = new Transportadora { RazaoSocial = "CargaSul Comex Ltda", Cnpj = "11222333000181", Uf = "RS", LimiteMensalCo2Kg = 20000m };
        db.Transportadoras.AddRange(tEcoLog, tRodoVerde, tCargaSul);

        // ----- Licenças ambientais -----
        db.Licencas.AddRange(
            new LicencaAmbiental { Transportadora = tEcoLog, NumeroLicenca = "CETESB-2025-0001", OrgaoEmissor = "CETESB", DataEmissao = hoje.AddMonths(-10), DataValidade = hoje.AddMonths(8), Status = StatusLicenca.Ativa },
            new LicencaAmbiental { Transportadora = tRodoVerde, NumeroLicenca = "IAT-2025-0042", OrgaoEmissor = "IAT-PR", DataEmissao = hoje.AddMonths(-11), DataValidade = hoje.AddDays(20), Status = StatusLicenca.Ativa }, // vence em breve
            new LicencaAmbiental { Transportadora = tCargaSul, NumeroLicenca = "FEPAM-2024-0777", OrgaoEmissor = "FEPAM-RS", DataEmissao = hoje.AddMonths(-14), DataValidade = hoje.AddMonths(-1), Status = StatusLicenca.Vencida } // irregular
        );

        // ----- Veículos -----
        var vEco1 = new Veiculo { Transportadora = tEcoLog, Placa = "ECO1A23", TipoCombustivel = TipoCombustivel.DieselS10, ConsumoMedioKmL = 3.2m, Ativo = true };
        var vEco2 = new Veiculo { Transportadora = tEcoLog, Placa = "ECO2B34", TipoCombustivel = TipoCombustivel.GasNatural, ConsumoMedioKmL = 4.0m, Ativo = true };
        var vRodo1 = new Veiculo { Transportadora = tRodoVerde, Placa = "RDV1C45", TipoCombustivel = TipoCombustivel.Diesel, ConsumoMedioKmL = 2.8m, Ativo = true };
        var vSul1 = new Veiculo { Transportadora = tCargaSul, Placa = "SUL1D56", TipoCombustivel = TipoCombustivel.Diesel, ConsumoMedioKmL = 2.5m, Ativo = true };
        db.Veiculos.AddRange(vEco1, vEco2, vRodo1, vSul1);

        // ----- Operações -----
        // Acumulador mensal por transportadora (chave: referência da transportadora + ano/mês).
        var acumulado = new Dictionary<(Transportadora, int, int), (decimal total, int qtd)>();
        var operacoes = new List<OperacaoLogistica>();

        void Registrar(Transportadora t, Veiculo v, DateTime data, string origem, string destino, decimal distancia, decimal carga)
        {
            var emissao = CalculadoraEmissao.Calcular(distancia, v);
            operacoes.Add(new OperacaoLogistica
            {
                Veiculo = v, DataOperacao = data, Origem = origem, Destino = destino,
                Modal = ModalTransporte.Rodoviario, DistanciaKm = distancia, CargaToneladas = carga,
                EmissaoCo2Kg = emissao
            });

            var chave = (t, data.Year, data.Month);
            acumulado.TryGetValue(chave, out var atual);
            acumulado[chave] = (atual.total + emissao, atual.qtd + 1);
        }

        Registrar(tEcoLog, vEco1, hoje.AddDays(-2), "Santos/SP", "Campinas/SP", 1800m, 22m);
        Registrar(tEcoLog, vEco2, hoje.AddDays(-1), "Campinas/SP", "Ribeirão Preto/SP", 1200m, 18m);
        Registrar(tEcoLog, vEco1, hoje.AddMonths(-1).AddDays(-3), "Santos/SP", "Bauru/SP", 1500m, 20m);

        // RodoVerde: volume alto para acionar alerta de teto.
        Registrar(tRodoVerde, vRodo1, hoje.AddDays(-5), "Curitiba/PR", "Foz do Iguaçu/PR", 8500m, 28m);
        Registrar(tRodoVerde, vRodo1, hoje.AddDays(-3), "Curitiba/PR", "Maringá/PR", 3500m, 25m);

        Registrar(tCargaSul, vSul1, hoje.AddMonths(-1).AddDays(-2), "Porto Alegre/RS", "Rio Grande/RS", 2600m, 30m);

        db.Operacoes.AddRange(operacoes);

        // ----- Acumulados mensais de CO2 -----
        foreach (var ((transportadora, ano, mes), valor) in acumulado)
        {
            db.Emissoes.Add(new EmissaoCarbono
            {
                Transportadora = transportadora,
                Ano = ano, Mes = mes,
                TotalCo2Kg = valor.total, QtdOperacoes = valor.qtd,
                AtualizadoEm = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
