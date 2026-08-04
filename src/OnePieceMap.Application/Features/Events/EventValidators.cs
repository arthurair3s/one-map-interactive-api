using FluentValidation;
using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Application.Features.Events;

public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventDtoValidator()
    {
        RuleFor(x => x.ArcIslandId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Type).NotEmpty().IsEnumName(typeof(EventType), caseSensitive: false);
        RuleFor(x => x.Order).GreaterThan(0);
    }
}

public class UpdateEventDtoValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventDtoValidator()
    {
        RuleFor(x => x.ArcIslandId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotNull();
        RuleFor(x => x.Type).NotEmpty().IsEnumName(typeof(EventType), caseSensitive: false);
        RuleFor(x => x.Order).GreaterThan(0);
    }
}
