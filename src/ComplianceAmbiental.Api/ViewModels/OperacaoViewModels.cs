using ComplianceAmbiental.Api.Models;

namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>Dados de entrada para registrar uma nova operação logística.</summary>
public class OperacaoCreateViewModel
{
    public int VeiculoId { get; set; }
    public DateTime DataOperacao { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public ModalTransporte Modal { get; set; } = ModalTransporte.Rodoviario;
    public decimal DistanciaKm { get; set; }
    public decimal CargaToneladas { get; set; }
}

/// <summary>Representação de saída de uma operação logística com a emissão já calculada.</summary>
public class OperacaoViewModel
{
    public int Id { get; set; }
    public int VeiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Transportadora { get; set; } = string.Empty;
    public DateTime DataOperacao { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public string Modal { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
    public decimal CargaToneladas { get; set; }
    public decimal EmissaoCo2Kg { get; set; }

    public static OperacaoViewModel FromEntity(OperacaoLogistica op) => new()
    {
        Id = op.Id,
        VeiculoId = op.VeiculoId,
        Placa = op.Veiculo?.Placa ?? string.Empty,
        Transportadora = op.Veiculo?.Transportadora?.RazaoSocial ?? string.Empty,
        DataOperacao = op.DataOperacao,
        Origem = op.Origem,
        Destino = op.Destino,
        Modal = op.Modal.ToString(),
        DistanciaKm = op.DistanciaKm,
        CargaToneladas = op.CargaToneladas,
        EmissaoCo2Kg = op.EmissaoCo2Kg
    };
}

/// <summary>Filtros opcionais da listagem paginada de operações.</summary>
public class OperacaoFilterQuery : PaginationQuery
{
    public int? TransportadoraId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
