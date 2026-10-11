using ComplianceAmbiental.Api.ViewModels;
using FluentValidation;

namespace ComplianceAmbiental.Api.Validators;

/// <summary>Validação avançada dos dados de entrada de uma operação logística.</summary>
public class OperacaoCreateValidator : AbstractValidator<OperacaoCreateViewModel>
{
    public OperacaoCreateValidator()
    {
        RuleFor(x => x.VeiculoId)
            .GreaterThan(0).WithMessage("O veículo é obrigatório.");

        RuleFor(x => x.DataOperacao)
            .NotEmpty().WithMessage("A data da operação é obrigatória.")
            .Must(d => d <= DateTime.UtcNow.AddDays(1))
            .WithMessage("A data da operação não pode ser futura.");

        RuleFor(x => x.Origem)
            .NotEmpty().WithMessage("A origem é obrigatória.")
            .MaximumLength(120);

        RuleFor(x => x.Destino)
            .NotEmpty().WithMessage("O destino é obrigatório.")
            .MaximumLength(120);

        RuleFor(x => x.Modal)
            .IsInEnum().WithMessage("Modal de transporte inválido.");

        RuleFor(x => x.DistanciaKm)
            .GreaterThan(0).WithMessage("A distância deve ser maior que zero.")
            .LessThanOrEqualTo(50000).WithMessage("Distância acima do limite plausível (50.000 km).");

        RuleFor(x => x.CargaToneladas)
            .GreaterThanOrEqualTo(0).WithMessage("A carga não pode ser negativa.")
            .LessThanOrEqualTo(100000).WithMessage("Carga acima do limite plausível.");
    }
}
