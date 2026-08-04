using FluentValidation;

namespace OnePieceMap.Application.Features.EventParticipants;

public class CreateEventParticipantDtoValidator : AbstractValidator<CreateEventParticipantDto>
{
    public CreateEventParticipantDtoValidator()
    {
        RuleFor(x => x.CharacterVersionId).GreaterThan(0);
    }
}
