using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Exceptions;
using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Services;

/// <inheritdoc />
public class LicencaService : ILicencaService
{
    private readonly AppDbContext _db;

    public LicencaService(AppDbContext db) => _db = db;

    public async Task<PagedResultViewModel<LicencaStatusViewModel>> ListarStatusAsync(LicencaFilterQuery query, CancellationToken ct = default)
    {
        var hoje = DateTime.UtcNow.Date;

        var consulta = _db.Licencas
            .AsNoTracking()
            .Include(l => l.Transportadora)
            .AsQueryable();

        if (query.VenceEmDias is int dias)
        {
            var limite = hoje.AddDays(dias);
            consulta = consulta.Where(l => l.DataValidade <= limite);
        }

        if (query.ApenasIrregulares)
        {
            consulta = consulta.Where(l => l.Status != StatusLicenca.Ativa || l.DataValidade < hoje);
        }

        var total = await consulta.CountAsync(ct);

        var page = await consulta
            .OrderBy(l => l.DataValidade)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var itens = page.Select(l => MapStatus(l, hoje)).ToList();
        return new PagedResultViewModel<LicencaStatusViewModel>(itens, total, query.PageNumber, query.PageSize);
    }

    public async Task<LicencaStatusViewModel> RenovarAsync(int licencaId, LicencaRenovacaoViewModel input, CancellationToken ct = default)
    {
        if (input.NovaDataValidade.Date <= DateTime.UtcNow.Date)
            throw new BusinessRuleException("A nova data de validade deve ser futura.");

        var licenca = await _db.Licencas
            .Include(l => l.Transportadora)
            .FirstOrDefaultAsync(l => l.Id == licencaId, ct);

        if (licenca is null)
            throw new NotFoundException($"Licença {licencaId} não encontrada.");

        licenca.DataValidade = input.NovaDataValidade;
        licenca.Status = StatusLicenca.Ativa;
        if (!string.IsNullOrWhiteSpace(input.NovoNumeroLicenca))
            licenca.NumeroLicenca = input.NovoNumeroLicenca!;

        await _db.SaveChangesAsync(ct);

        return MapStatus(licenca, DateTime.UtcNow.Date);
    }

    private static LicencaStatusViewModel MapStatus(LicencaAmbiental l, DateTime hoje)
    {
        var diasParaVencer = (int)(l.DataValidade.Date - hoje).TotalDays;

        string situacao;
        if (l.Status == StatusLicenca.Cancelada || l.Status == StatusLicenca.Suspensa)
            situacao = "IRREGULAR";
        else if (l.DataValidade.Date < hoje || l.Status == StatusLicenca.Vencida)
            situacao = "VENCIDA";
        else if (diasParaVencer <= 30)
            situacao = "A_VENCER";
        else
            situacao = "REGULAR";

        return new LicencaStatusViewModel
        {
            Id = l.Id,
            TransportadoraId = l.TransportadoraId,
            Transportadora = l.Transportadora?.RazaoSocial ?? string.Empty,
            NumeroLicenca = l.NumeroLicenca,
            OrgaoEmissor = l.OrgaoEmissor,
            Status = l.Status.ToString(),
            DataValidade = l.DataValidade,
            DiasParaVencer = diasParaVencer,
            SituacaoCompliance = situacao
        };
    }
}
