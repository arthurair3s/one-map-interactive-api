using FluentValidation;

namespace OnePieceMap.Application.Features.Islands;

public class CreateIslandDtoValidator : AbstractValidator<CreateIslandDto>
{
    public CreateIslandDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.ModelUrl).NotEmpty();
        RuleFor(x => x.ThumbnailUrl).NotEmpty();
        RuleFor(x => x.Scale).GreaterThan(0);
    }
}

public class UpdateIslandDtoValidator : AbstractValidator<UpdateIslandDto>
{
    public UpdateIslandDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.ModelUrl).NotEmpty();
        RuleFor(x => x.ThumbnailUrl).NotEmpty();
        RuleFor(x => x.Scale).GreaterThan(0);
    }
}
