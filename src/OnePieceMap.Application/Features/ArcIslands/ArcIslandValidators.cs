using FluentValidation;

namespace OnePieceMap.Application.Features.ArcIslands;

public class CreateArcIslandDtoValidator : AbstractValidator<CreateArcIslandDto>
{
    public CreateArcIslandDtoValidator()
    {
        RuleFor(x => x.IslandId).GreaterThan(0);
        RuleFor(x => x.Order).GreaterThan(0);
    }
}
