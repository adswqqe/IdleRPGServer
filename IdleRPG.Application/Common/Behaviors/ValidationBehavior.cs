using FluentValidation;
using MediatR;

namespace IdleRPG.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior - 모든 요청에 자동 검증 적용
/// </summary>
/// <remarks>
/// 핵심 개념:
/// - IPipelineBehavior는 MediatR의 미들웨어 같은 역할
/// - 모든 IRequest가 Handler에 도달하기 전에 이 Behavior를 거침
/// - 여러 Behavior를 체인처럼 연결 가능 (Logging → Validation → Transaction → Handler)
///
/// 동작 흐름:
/// Controller → MediatR.Send() → [ValidationBehavior] → Handler
///                                      ↓
///                               검증 실패 시 예외 던짐
///                               검증 성공 시 next() 호출
/// </remarks>
/// <typeparam name="TRequest">요청 타입 (Command 또는 Query)</typeparam>
/// <typeparam name="TResponse">응답 타입</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// 생성자 - DI 컨테이너가 해당 Request에 대한 모든 Validator를 주입
    /// </summary>
    /// <remarks>
    /// 예: AcceptQuestCommand 요청 시
    /// → IValidator&lt;AcceptQuestCommand&gt;를 구현한 모든 클래스가 주입됨
    /// → AcceptQuestCommandValidator가 자동으로 들어옴
    /// </remarks>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. 해당 Request에 대한 Validator가 없으면 바로 통과
        if (!_validators.Any())
        {
            return await next();
        }

        // 2. 모든 Validator 실행
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // 3. 실패한 검증 결과만 수집
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // 4. 검증 실패가 있으면 예외 던지기
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // 5. 검증 성공 → 다음 단계로 진행 (Handler 또는 다른 Behavior)
        return await next();
    }
}
