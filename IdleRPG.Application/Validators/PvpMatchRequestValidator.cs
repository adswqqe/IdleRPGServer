using FluentValidation;
using IdleRPG.Application.DTOs.Pvp;

namespace IdleRPG.Application.Validators;

/// <summary>
/// PVP 매치 시작 요청 검증기
/// </summary>
public class PvpMatchRequestValidator : AbstractValidator<PvpMatchRequestDto>
{
    public PvpMatchRequestValidator()
    {
        RuleFor(x => x.CharacterId)
            .NotEmpty()
            .WithMessage("CharacterId는 필수입니다.")
            .Must(BeValidGuid)
            .WithMessage("CharacterId는 유효한 Guid여야 합니다.");
    }

    /// <summary>
    /// Guid가 Empty가 아닌지 검증
    /// </summary>
    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
