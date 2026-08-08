using FluentValidation;
using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Application.Features.CharacterVersions;

public class CreateCharacterVersionDtoValidator : AbstractValidator<CreateCharacterVersionDto>
{
    public CreateCharacterVersionDtoValidator()
    {
        RuleFor(x => x.ArcId).GreaterThan(0);
        RuleFor(x => x.Alias).NotEmpty();
        RuleFor(x => x.Epithet).NotNull();
        RuleFor(x => x.Status).NotEmpty().IsEnumName(typeof(CharacterStatus), caseSensitive: false);
        RuleFor(x => x.FactionId).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty();
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Bounty).GreaterThanOrEqualTo(0).When(x => x.Bounty.HasValue);
    }
}

public class UpdateCharacterVersionDtoValidator : AbstractValidator<UpdateCharacterVersionDto>
{
    public UpdateCharacterVersionDtoValidator()
    {
        RuleFor(x => x.ArcId).GreaterThan(0);
        RuleFor(x => x.Alias).NotEmpty();
        RuleFor(x => x.Epithet).NotNull();
        RuleFor(x => x.Status).NotEmpty().IsEnumName(typeof(CharacterStatus), caseSensitive: false);
        RuleFor(x => x.FactionId).GreaterThan(0);
        RuleFor(x => x.ImageUrl).NotEmpty();
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Bounty).GreaterThanOrEqualTo(0).When(x => x.Bounty.HasValue);
    }
}
