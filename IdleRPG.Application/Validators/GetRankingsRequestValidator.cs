using FluentValidation;
using IdleRPG.Application.DTOs.Pvp;

namespace IdleRPG.Application.Validators;

/// <summary>
/// PVP 랭킹 조회 요청 검증기
/// </summary>
public class GetRankingsRequestValidator : AbstractValidator<GetRankingsRequest>
{
    public GetRankingsRequestValidator()
    {
        // SeasonId 검증: 양수여야 함 (nullable이므로 값이 있을 때만 검증)
        RuleFor(x => x.SeasonId)
            .GreaterThan(0)
            .WithMessage("SeasonId는 0보다 커야 합니다.")
            .When(x => x.SeasonId.HasValue);

        // Top 검증: 1~1000 범위
        RuleFor(x => x.Top)
            .InclusiveBetween(1, 1000)
            .WithMessage("Top은 1에서 1000 사이여야 합니다.")
            .When(x => x.Top.HasValue);

        // Range 검증: 1~50 범위
        RuleFor(x => x.Range)
            .InclusiveBetween(1, 50)
            .WithMessage("Range는 1에서 50 사이여야 합니다.")
            .When(x => x.Range.HasValue);

        // Tier 검증: Enum 값이어야 함
        RuleFor(x => x.Tier)
            .IsInEnum()
            .WithMessage("Tier는 유효한 값이어야 합니다 (Bronze, Silver, Gold, Platinum, Diamond).")
            .When(x => x.Tier.HasValue);

        // Page 검증: 1 이상
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page는 1 이상이어야 합니다.");

        // PageSize 검증: 1~100 범위
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize는 1에서 100 사이여야 합니다.");
    }
}
