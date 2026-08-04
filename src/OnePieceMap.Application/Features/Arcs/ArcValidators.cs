using FluentValidation;

namespace OnePieceMap.Application.Features.Arcs;

public class CreateArcDtoValidator : AbstractValidator<CreateArcDto>
{
    public CreateArcDtoValidator()
    {
        RuleFor(x => x.SagaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Order).GreaterThan(0);
        RuleFor(x => x.GlobalOrder).GreaterThan(0);
    }
}

public class UpdateArcDtoValidator : AbstractValidator<UpdateArcDto>
{
    public UpdateArcDtoValidator()
    {
        RuleFor(x => x.SagaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Order).GreaterThan(0);
        RuleFor(x => x.GlobalOrder).GreaterThan(0);
    }
}
