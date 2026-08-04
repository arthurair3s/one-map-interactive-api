using FluentValidation;

namespace OnePieceMap.Application.Features.Sagas;

public class CreateSagaDtoValidator : AbstractValidator<CreateSagaDto>
{
    public CreateSagaDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Order).GreaterThan(0);
    }
}

public class UpdateSagaDtoValidator : AbstractValidator<UpdateSagaDto>
{
    public UpdateSagaDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Order).GreaterThan(0);
    }
}
