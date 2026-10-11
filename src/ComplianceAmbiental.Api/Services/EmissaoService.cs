using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Services;

/// <inheritdoc />
public class EmissaoService : IEmissaoService
{
    private readonly AppDbContext _db;

    public EmissaoService(AppDbContext db) => _db = db;

    public async Task<PagedResultViewModel<EmissaoConsolidadoViewModel>> ConsolidarAsync(EmissaoFilterQuery query, CancellationToken ct = default)
    {
        var consulta = _db.Emissoes
            .AsNoTracking()
            .Include(e => e.Transportadora)
            .AsQueryable();

        if (query.Ano is int ano)
            consulta = consulta.Where(e => e.Ano == ano);
        if (query.Mes is int mes)
            consulta = consulta.Where(e => e.Mes == mes);
        if (query.TransportadoraId is int tid)
            consulta = consulta.Where(e => e.TransportadoraId == tid);

        var total = await consulta.CountAsync(ct);

        var page = await consulta
            .OrderByDescending(e => e.Ano)
            .ThenByDescending(e => e.Mes)
            .ThenByDescending(e => e.TotalCo2Kg)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var itens = page.Select(MapConsolidado).ToList();
        return new PagedResultViewModel<EmissaoConsolidadoViewModel>(itens, total, query.PageNumber, query.PageSize);
    }

    public async Task<PagedResultViewModel<AlertaEmissaoViewModel>> ListarAlertasAsync(EmissaoFilterQuery query, CancellationToken ct = default)
    {
        // Considera-se alerta quando o acumulado mensal atinge 80% ou mais do teto.
        const decimal limiarAlerta = 0.80m;

        var consulta = _db.Emissoes
            .AsNoTracking()
            .Include(e => e.Transportadora)
            .Where(e => e.Transportadora!.LimiteMensalCo2Kg > 0
                       && e.TotalCo2Kg >= e.Transportadora.LimiteMensalCo2Kg * limiarAlerta);

        if (query.Ano is int ano)
            consulta = consulta.Where(e => e.Ano == ano);
        if (query.Mes is int mes)
            consulta = consulta.Where(e => e.Mes == mes);
        if (query.TransportadoraId is int tid)
            consulta = consulta.Where(e => e.TransportadoraId == tid);

        var total = await consulta.CountAsync(ct);

        var page = await consulta
            .OrderByDescending(e => e.TotalCo2Kg)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var itens = page.Select(MapAlerta).ToList();
        return new PagedResultViewModel<AlertaEmissaoViewModel>(itens, total, query.PageNumber, query.PageSize);
    }

    private static EmissaoConsolidadoViewModel MapConsolidado(EmissaoCarbono e)
    {
        var limite = e.Transportadora?.LimiteMensalCo2Kg ?? 0m;
        return new EmissaoConsolidadoViewModel
        {
            TransportadoraId = e.TransportadoraId,
            Transportadora = e.Transportadora?.RazaoSocial ?? string.Empty,
            Ano = e.Ano,
            Mes = e.Mes,
            TotalCo2Kg = e.TotalCo2Kg,
            QtdOperacoes = e.QtdOperacoes,
            LimiteMensalCo2Kg = limite,
            PercentualDoLimite = limite > 0 ? Math.Round(e.TotalCo2Kg / limite * 100, 1) : 0m
        };
    }

    private static AlertaEmissaoViewModel MapAlerta(EmissaoCarbono e)
    {
        var limite = e.Transportadora?.LimiteMensalCo2Kg ?? 0m;
        var percentual = limite > 0 ? Math.Round(e.TotalCo2Kg / limite * 100, 1) : 0m;
        var excedente = Math.Max(0, e.TotalCo2Kg - limite);

        var severidade = percentual >= 100 ? "CRITICO"
                       : percentual >= 90 ? "ALTO"
                       : "ATENCAO";

        return new AlertaEmissaoViewModel
        {
            TransportadoraId = e.TransportadoraId,
            Transportadora = e.Transportadora?.RazaoSocial ?? string.Empty,
            Ano = e.Ano,
            Mes = e.Mes,
            TotalCo2Kg = e.TotalCo2Kg,
            LimiteMensalCo2Kg = limite,
            ExcedenteCo2Kg = excedente,
            PercentualDoLimite = percentual,
            Severidade = severidade
        };
    }
}
