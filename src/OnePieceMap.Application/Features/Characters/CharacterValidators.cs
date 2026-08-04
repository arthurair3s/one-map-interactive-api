using FluentValidation;

namespace OnePieceMap.Application.Features.Characters;

public class CreateCharacterDtoValidator : AbstractValidator<CreateCharacterDto>
{
    public CreateCharacterDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}

public class UpdateCharacterDtoValidator : AbstractValidator<UpdateCharacterDto>
{
    public UpdateCharacterDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty();
    }
}
