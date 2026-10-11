using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Exceptions;
using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Services;

/// <inheritdoc />
public class TransportadoraService : ITransportadoraService
{
    private readonly AppDbContext _db;

    public TransportadoraService(AppDbContext db) => _db = db;

    public async Task<PagedResultViewModel<TransportadoraComplianceViewModel>> ListarAsync(TransportadoraFilterQuery query, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow.Date;
        var ano = hoje.Year;
        var mes = hoje.Month;

        var consulta = _db.Transportadoras.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Uf))
            consulta = consulta.Where(t => t.Uf == query.Uf);

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var termo = query.Busca.Trim();
            consulta = consulta.Where(t => t.RazaoSocial.Contains(termo) || t.Cnpj.Contains(termo));
        }

        var total = await consulta.CountAsync(ct);

        var page = await consulta
            .OrderBy(t => t.RazaoSocial)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var ids = page.Select(t => t.Id).ToList();

        // Agregações em consultas únicas para evitar N+1.
        var licencasVigentes = await _db.Licencas
            .AsNoTracking()
            .Where(l => ids.Contains(l.TransportadoraId)
                       && l.Status == StatusLicenca.Ativa
                       && l.DataValidade >= hoje)
            .Select(l => l.TransportadoraId)
            .Distinct()
            .ToListAsync(ct);

        var qtdLicencas = await _db.Licencas
            .AsNoTracking()
            .Where(l => ids.Contains(l.TransportadoraId))
            .GroupBy(l => l.TransportadoraId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .ToListAsync(ct);

        var qtdVeiculos = await _db.Veiculos
            .AsNoTracking()
            .Where(v => ids.Contains(v.TransportadoraId))
            .GroupBy(v => v.TransportadoraId)
            .Select(g => new { Id = g.Key, Qtd = g.Count() })
            .ToListAsync(ct);

        var emissoesMes = await _db.Emissoes
            .AsNoTracking()
            .Where(e => ids.Contains(e.TransportadoraId) && e.Ano == ano && e.Mes == mes)
            .Select(e => new { e.TransportadoraId, e.TotalCo2Kg })
            .ToListAsync(ct);

        var vigentesSet = licencasVigentes.ToHashSet();
        var licencasMap = qtdLicencas.ToDictionary(x => x.Id, x => x.Qtd);
        var veiculosMap = qtdVeiculos.ToDictionary(x => x.Id, x => x.Qtd);
        var emissaoMap = emissoesMes.ToDictionary(x => x.TransportadoraId, x => x.TotalCo2Kg);

        var itens = page.Select(t =>
        {
            var possuiVigente = vigentesSet.Contains(t.Id);
            emissaoMap.TryGetValue(t.Id, out var co2Mes);
            return new TransportadoraComplianceViewModel
            {
                Id = t.Id,
                RazaoSocial = t.RazaoSocial,
                Cnpj = t.Cnpj,
                Uf = t.Uf,
                LimiteMensalCo2Kg = t.LimiteMensalCo2Kg,
                PossuiLicencaVigente = possuiVigente,
                QtdLicencas = licencasMap.GetValueOrDefault(t.Id),
                QtdVeiculos = veiculosMap.GetValueOrDefault(t.Id),
                ScoreCompliance = CalcularScore(possuiVigente, co2Mes, t.LimiteMensalCo2Kg)
            };
        }).ToList();

        return new PagedResultViewModel<TransportadoraComplianceViewModel>(itens, total, query.PageNumber, query.PageSize);
    }

    public async Task<DashboardEsgViewModel> ObterDashboardAsync(int transportadoraId, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow.Date;

        var transportadora = await _db.Transportadoras
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transportadoraId, ct);

        if (transportadora is null)
            throw new NotFoundException($"Transportadora {transportadoraId} não encontrada.");

        var possuiVigente = await _db.Licencas
            .AsNoTracking()
            .AnyAsync(l => l.TransportadoraId == transportadoraId
                          && l.Status == StatusLicenca.Ativa
                          && l.DataValidade >= hoje, ct);

        var qtdVeiculosAtivos = await _db.Veiculos
            .AsNoTracking()
            .CountAsync(v => v.TransportadoraId == transportadoraId && v.Ativo, ct);

        var qtdOperacoes = await _db.Operacoes
            .AsNoTracking()
            .CountAsync(o => o.Veiculo!.TransportadoraId == transportadoraId, ct);

        var emissoes = await _db.Emissoes
            .AsNoTracking()
            .Where(e => e.TransportadoraId == transportadoraId)
            .OrderByDescending(e => e.Ano).ThenByDescending(e => e.Mes)
            .ToListAsync(ct);

        var co2Total = emissoes.Sum(e => e.TotalCo2Kg);
        var co2MesAtual = emissoes
            .Where(e => e.Ano == hoje.Year && e.Mes == hoje.Month)
            .Sum(e => e.TotalCo2Kg);

        var limite = transportadora.LimiteMensalCo2Kg;
        var percentualMes = limite > 0 ? Math.Round(co2MesAtual / limite * 100, 1) : 0m;

        var historico = emissoes.Select(e => new EmissaoConsolidadoViewModel
        {
            TransportadoraId = e.TransportadoraId,
            Transportadora = transportadora.RazaoSocial,
            Ano = e.Ano,
            Mes = e.Mes,
            TotalCo2Kg = e.TotalCo2Kg,
            QtdOperacoes = e.QtdOperacoes,
            LimiteMensalCo2Kg = limite,
            PercentualDoLimite = limite > 0 ? Math.Round(e.TotalCo2Kg / limite * 100, 1) : 0m
        }).ToList();

        return new DashboardEsgViewModel
        {
            TransportadoraId = transportadora.Id,
            Transportadora = transportadora.RazaoSocial,
            PossuiLicencaVigente = possuiVigente,
            QtdVeiculosAtivos = qtdVeiculosAtivos,
            QtdOperacoesTotal = qtdOperacoes,
            Co2TotalKg = co2Total,
            Co2MesAtualKg = co2MesAtual,
            LimiteMensalCo2Kg = limite,
            PercentualDoLimiteMesAtual = percentualMes,
            ScoreCompliance = CalcularScore(possuiVigente, co2MesAtual, limite),
            HistoricoMensal = historico
        };
    }

    /// <summary>
    /// Score de compliance (0 a 100): penaliza ausência de licença vigente e
    /// ultrapassagem do teto mensal de CO2.
    /// </summary>
    private static int CalcularScore(bool possuiLicencaVigente, decimal co2Mes, decimal limite)
    {
        var score = 100;

        if (!possuiLicencaVigente)
            score -= 50;

        if (limite > 0)
        {
            var percentual = co2Mes / limite;
            if (percentual > 1m)
                score -= 40;
            else if (percentual >= 0.8m)
                score -= 15;
        }

        return Math.Clamp(score, 0, 100);
    }
}
