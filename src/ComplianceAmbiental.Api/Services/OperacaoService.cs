using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Exceptions;
using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Services;

/// <inheritdoc />
public class OperacaoService : IOperacaoService
{
    private readonly AppDbContext _db;

    public OperacaoService(AppDbContext db) => _db = db;

    public async Task<OperacaoViewModel> RegistrarAsync(OperacaoCreateViewModel input, CancellationToken ct = default)
    {
        var veiculo = await _db.Veiculos
            .Include(v => v.Transportadora)
            .FirstOrDefaultAsync(v => v.Id == input.VeiculoId, ct);

        if (veiculo is null)
            throw new NotFoundException($"Veículo {input.VeiculoId} não encontrado.");

        if (!veiculo.Ativo)
            throw new BusinessRuleException($"Veículo {veiculo.Placa} está inativo e não pode operar.");

        var transportadora = veiculo.Transportadora
            ?? throw new BusinessRuleException("Veículo sem transportadora associada.");

        // Regra de compliance: bloqueia operações sem licença ambiental vigente.
        var possuiLicencaVigente = await _db.Licencas
            .AsNoTracking()
            .AnyAsync(l => l.TransportadoraId == transportadora.Id
                          && l.Status == StatusLicenca.Ativa
                          && l.DataEmissao <= input.DataOperacao
                          && l.DataValidade >= input.DataOperacao, ct);

        if (!possuiLicencaVigente)
            throw new BusinessRuleException(
                $"Transportadora {transportadora.RazaoSocial} não possui licença ambiental vigente na data da operação. Operação bloqueada.");

        // Cálculo automático da emissão de CO2.
        var emissao = CalculadoraEmissao.Calcular(input.DistanciaKm, veiculo);

        var operacao = new OperacaoLogistica
        {
            VeiculoId = veiculo.Id,
            DataOperacao = input.DataOperacao,
            Origem = input.Origem,
            Destino = input.Destino,
            Modal = input.Modal,
            DistanciaKm = input.DistanciaKm,
            CargaToneladas = input.CargaToneladas,
            EmissaoCo2Kg = emissao
        };

        _db.Operacoes.Add(operacao);

        // Consolidação em tempo real do acumulado mensal.
        await AtualizarAcumuladoMensalAsync(transportadora.Id, input.DataOperacao, emissao, ct);

        await _db.SaveChangesAsync(ct);

        operacao.Veiculo = veiculo;
        return OperacaoViewModel.FromEntity(operacao);
    }

    private async Task AtualizarAcumuladoMensalAsync(int transportadoraId, DateTime data, decimal emissao, CancellationToken ct)
    {
        var acumulado = await _db.Emissoes
            .FirstOrDefaultAsync(e => e.TransportadoraId == transportadoraId
                                     && e.Ano == data.Year
                                     && e.Mes == data.Month, ct);

        if (acumulado is null)
        {
            _db.Emissoes.Add(new EmissaoCarbono
            {
                TransportadoraId = transportadoraId,
                Ano = data.Year,
                Mes = data.Month,
                TotalCo2Kg = emissao,
                QtdOperacoes = 1,
                AtualizadoEm = DateTime.UtcNow
            });
        }
        else
        {
            acumulado.TotalCo2Kg += emissao;
            acumulado.QtdOperacoes += 1;
            acumulado.AtualizadoEm = DateTime.UtcNow;
        }
    }

    public async Task<PagedResultViewModel<OperacaoViewModel>> ListarAsync(OperacaoFilterQuery query, CancellationToken ct = default)
    {
        // Consulta base com projeção e sem tracking para máxima performance.
        var consulta = _db.Operacoes
            .AsNoTracking()
            .Include(o => o.Veiculo!)
                .ThenInclude(v => v.Transportadora)
            .AsQueryable();

        if (query.TransportadoraId is int tid)
            consulta = consulta.Where(o => o.Veiculo!.TransportadoraId == tid);

        if (query.DataInicio is DateTime ini)
            consulta = consulta.Where(o => o.DataOperacao >= ini);

        if (query.DataFim is DateTime fim)
            consulta = consulta.Where(o => o.DataOperacao <= fim);

        var total = await consulta.CountAsync(ct);

        // Otimização: paginação no banco (Skip/Take) antes de materializar.
        var entidades = await consulta
            .OrderByDescending(o => o.DataOperacao)
            .ThenByDescending(o => o.Id)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var itens = entidades.Select(OperacaoViewModel.FromEntity).ToList();

        return new PagedResultViewModel<OperacaoViewModel>(itens, total, query.PageNumber, query.PageSize);
    }
}
