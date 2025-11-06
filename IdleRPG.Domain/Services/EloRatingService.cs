namespace IdleRPG.Domain.Services
{
    /// <summary>
    /// ELO 레이팅 계산 Domain Service (순수 비즈니스 로직)
    ///
    /// [설계 원칙]
    /// - 외부 의존성 없음 (Repository, Infrastructure 의존 금지)
    /// - 순수 수학 계산 로직만 포함
    /// - 단위 테스트 용이성 (입력 → 출력 명확)
    ///
    /// [ELO 알고리즘 개요]
    /// 1. 기대 승률 계산: E_A = 1 / (1 + 10^((R_B - R_A) / 400))
    /// 2. 레이팅 변화: ΔR_A = K * (S_A - E_A)
    ///    - S_A = 실제 결과 (승리 1, 패배 0)
    ///    - K = K-Factor (레이팅 변화량 조절)
    /// 3. 새 레이팅: R'_A = R_A + ΔR_A
    /// </summary>
    public class EloRatingService
    {
        // Decision: Option A (하드코딩) 채택
        // Reasoning: MVP 단계에서 K-Factor 변경 계획 없음, 단순성 우선
        // Future: Phase 2에서 appsettings.json으로 이동 예정
        private const int DefaultKFactor = 32;

        /// <summary>
        /// 승자와 패자의 새로운 레이팅을 계산합니다 (ELO 표준 알고리즘)
        /// </summary>
        /// <param name="winnerRating">승자의 현재 레이팅</param>
        /// <param name="loserRating">패자의 현재 레이팅</param>
        /// <param name="kFactor">K-Factor (기본값 32, 레이팅 변화량 조절)</param>
        /// <returns>(승자 새 레이팅, 패자 새 레이팅)</returns>
        /// <exception cref="ArgumentException">레이팅이 음수인 경우</exception>
        public (int winnerNewRating, int loserNewRating) CalculateNewRatings(
            int winnerRating,
            int loserRating,
            int kFactor = DefaultKFactor)
        {
            // Input Validation
            if (winnerRating < 0)
                throw new ArgumentException("Winner rating must be non-negative", nameof(winnerRating));

            if (loserRating < 0)
                throw new ArgumentException("Loser rating must be non-negative", nameof(loserRating));

            if (kFactor <= 0)
                throw new ArgumentException("K-Factor must be positive", nameof(kFactor));

            // Step 1: 승자의 기대 승률 계산
            double expectedWinner = CalculateExpectedScore(winnerRating, loserRating);

            // Step 2: 패자의 기대 승률 계산
            double expectedLoser = CalculateExpectedScore(loserRating, winnerRating);

            // Step 3: 레이팅 변화 계산
            // 승자: 실제 결과 1 (승리) - 기대 승률
            // 패자: 실제 결과 0 (패배) - 기대 승률
            double winnerChange = kFactor * (1.0 - expectedWinner);
            double loserChange = kFactor * (0.0 - expectedLoser);

            // Step 4: 새 레이팅 계산 (정수 반올림)
            int winnerNewRating = (int)Math.Round(winnerRating + winnerChange);
            int loserNewRating = (int)Math.Round(loserRating + loserChange);

            // Step 5: 최소값 0 보장 (음수 방지)
            winnerNewRating = Math.Max(0, winnerNewRating);
            loserNewRating = Math.Max(0, loserNewRating);

            return (winnerNewRating, loserNewRating);
        }

        /// <summary>
        /// 기대 승률 계산 (ELO 표준 공식)
        /// E_A = 1 / (1 + 10^((R_B - R_A) / 400))
        /// </summary>
        /// <param name="playerRating">플레이어 A의 레이팅</param>
        /// <param name="opponentRating">플레이어 B의 레이팅</param>
        /// <returns>플레이어 A의 기대 승률 (0.0 ~ 1.0)</returns>
        private double CalculateExpectedScore(int playerRating, int opponentRating)
        {
            // ELO 표준 공식: 1 / (1 + 10^((R_B - R_A) / 400))
            double exponent = (opponentRating - playerRating) / 400.0;
            double denominator = 1.0 + Math.Pow(10, exponent);
            return 1.0 / denominator;
        }
    }
}
