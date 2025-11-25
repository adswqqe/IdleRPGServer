using FluentValidation;

namespace IdleRPG.Application.Quests.Commands.AcceptQuest;

/// <summary>
/// AcceptQuestCommand 검증기
/// </summary>
/// <remarks>
/// FluentValidation 사용법:
/// - AbstractValidator&lt;T&gt; 상속
/// - 생성자에서 RuleFor()로 검증 규칙 정의
/// - DI에 등록하면 ValidationBehavior가 자동으로 찾아서 실행
///
/// 주요 메서드:
/// - RuleFor(x => x.Property): 특정 프로퍼티에 대한 규칙 시작
/// - .NotEmpty(): null 또는 빈 값 불허
/// - .NotEqual(value): 특정 값과 같으면 안 됨
/// - .GreaterThan(n): n보다 커야 함
/// - .WithMessage("..."): 커스텀 에러 메시지
/// </remarks>
public class AcceptQuestCommandValidator : AbstractValidator<AcceptQuestCommand>
{
    public AcceptQuestCommandValidator()
    {
        // CharacterId: 빈 GUID 불허
        RuleFor(x => x.CharacterId).NotEmpty();

        // QuestTemplateId: 0보다 커야 함
        RuleFor(x => x.QuestTemplateId).GreaterThan(0);
    }
}
