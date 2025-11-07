using FluentValidation;
using IdleRPG.Application.DTOs.Pvp;

namespace IdleRPG.Application.Validators;

/// <summary>
/// PVP 매치 히스토리 조회 요청 검증기
/// </summary>
public class GetMatchHistoryRequestValidator : AbstractValidator<GetMatchHistoryRequest>
{
    public GetMatchHistoryRequestValidator()
    {
        // CharacterId 검증: 비어있지 않고 유효한 Guid
        RuleFor(x => x.CharacterId)
            .NotEmpty()
            .WithMessage("CharacterId는 필수입니다.")
            .Must(BeValidGuid)
            .WithMessage("CharacterId는 유효한 Guid여야 합니다.");

        // SeasonId 검증: 양수여야 함 (nullable이므로 값이 있을 때만 검증)
        RuleFor(x => x.SeasonId)
            .GreaterThan(0)
            .WithMessage("SeasonId는 0보다 커야 합니다.")
            .When(x => x.SeasonId.HasValue);

        // Page 검증: 1 이상
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page는 1 이상이어야 합니다.");

        // PageSize 검증: 1~50 범위
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize는 1에서 50 사이여야 합니다.");
    }

    /// <summary>
    /// Guid가 Empty가 아닌지 검증
    /// </summary>
    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
