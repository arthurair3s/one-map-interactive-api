using FluentValidation;

namespace OnePieceMap.Application.Features.Factions;

public class CreateFactionDtoValidator : AbstractValidator<CreateFactionDto>
{
    public CreateFactionDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class UpdateFactionDtoValidator : AbstractValidator<UpdateFactionDto>
{
    public UpdateFactionDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}
