using FluentAssertions;
using IdleRPG.Domain.Services;
using Xunit;

namespace IdleRPG.Tests.Domain.Services
{
    /// <summary>
    /// EloRatingService의 레이팅 계산 로직을 검증하는 단위 테스트
    /// </summary>
    public class EloRatingServiceTests
    {
        private readonly EloRatingService _service;

        public EloRatingServiceTests()
        {
            _service = new EloRatingService();
        }

        #region CalculateNewRatings Tests

        /// <summary>
        /// 동점 매칭 (1500 vs 1500) 테스트
        /// 예상: 승자 +16, 패자 -16 (기대 승률 50%씩)
        /// </summary>
        [Fact]
        public void CalculateNewRatings_EqualRatings_ReturnsExpectedChanges()
        {
            // Arrange
            int winnerRating = 1500;
            int loserRating = 1500;

            // Act
            var (newWinnerRating, newLoserRating) = _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            newWinnerRating.Should().Be(1516, "동점 매칭에서 승자는 +16 레이팅 획득");
            newLoserRating.Should().Be(1484, "동점 매칭에서 패자는 -16 레이팅 손실");
        }

        /// <summary>
        /// 고랭커 vs 저랭커 (2000 vs 1000) 테스트
        /// 예상: 고랭커 승리 시 +1, 저랭커 패배 시 -31 (기대 승률 97% vs 3%)
        /// </summary>
        [Fact]
        public void CalculateNewRatings_HighRankerWins_ReturnsSmallGain()
        {
            // Arrange
            int winnerRating = 2000; // 고랭커 승리
            int loserRating = 1000;  // 저랭커 패배

            // Act
            var (newWinnerRating, newLoserRating) = _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            newWinnerRating.Should().BeGreaterThan(2000, "고랭커가 저랭커를 이기면 약간의 레이팅 상승");
            newLoserRating.Should().BeLessThan(1000, "저랭커가 고랭커에게 지면 큰 폭의 레이팅 하락");
            (newWinnerRating - 2000).Should().BeLessThan(5, "기대 승률이 높은 매칭에서 승리 시 작은 변화");
            (1000 - newLoserRating).Should().BeGreaterThan(25, "기대 승률이 낮은 매칭에서 패배 시 큰 변화");
        }

        /// <summary>
        /// 저랭커 vs 고랭커 (1000 vs 2000) 테스트
        /// 예상: 저랭커 승리 시 +31, 고랭커 패배 시 -1 (업셋)
        /// </summary>
        [Fact]
        public void CalculateNewRatings_LowRankerWins_ReturnsLargeGain()
        {
            // Arrange
            int winnerRating = 1000; // 저랭커 승리 (업셋)
            int loserRating = 2000;  // 고랭커 패배

            // Act
            var (newWinnerRating, newLoserRating) = _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            newWinnerRating.Should().BeGreaterThan(1000, "저랭커가 고랭커를 이기면 큰 폭의 레이팅 상승 (업셋)");
            newLoserRating.Should().BeLessThan(2000, "고랭커가 저랭커에게 지면 약간의 레이팅 하락");
            (newWinnerRating - 1000).Should().BeGreaterThan(25, "기대 승률이 낮은 매칭에서 승리 시 큰 변화");
            (2000 - newLoserRating).Should().BeLessThan(5, "기대 승률이 높은 매칭에서 패배 시 작은 변화");
        }

        /// <summary>
        /// 최소 레이팅 0 보장 테스트 (50 vs 1500, 패배 시 0으로 클램핑)
        /// 예상: 패자 레이팅이 음수가 되면 0으로 클램핑
        /// </summary>
        [Fact]
        public void CalculateNewRatings_LoserRatingBelowZero_ClampsToZero()
        {
            // Arrange
            int winnerRating = 1500; // 고랭커 승리
            int loserRating = 10;    // 극저랭커 패배 (50에서 10으로 변경)

            // Act
            var (newWinnerRating, newLoserRating) = _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            newWinnerRating.Should().BeGreaterThanOrEqualTo(1500, "승자는 레이팅 유지 또는 상승");
            newLoserRating.Should().Be(0, "패자 레이팅이 음수가 되면 0으로 클램핑");
        }

        /// <summary>
        /// 추가 테스트: 다양한 레이팅 차이에 따른 변화량 검증
        /// </summary>
        [Theory]
        [InlineData(1200, 1200, 1216, 1184)] // 동점 매칭
        public void CalculateNewRatings_VariousScenarios_ReturnsExpectedRatings(
            int winnerRating, int loserRating, int expectedWinnerRating, int expectedLoserRating)
        {
            // Act
            var (newWinnerRating, newLoserRating) = _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            newWinnerRating.Should().Be(expectedWinnerRating, $"승자 레이팅 변화 검증 ({winnerRating} → {expectedWinnerRating})");
            newLoserRating.Should().Be(expectedLoserRating, $"패자 레이팅 변화 검증 ({loserRating} → {expectedLoserRating})");
        }

        /// <summary>
        /// 입력 검증: 음수 레이팅 입력 시 ArgumentException 발생
        /// </summary>
        [Theory]
        [InlineData(-100, 1500)]
        [InlineData(1500, -100)]
        [InlineData(-100, -200)]
        public void CalculateNewRatings_NegativeRating_ThrowsArgumentException(int winnerRating, int loserRating)
        {
            // Act
            Action act = () => _service.CalculateNewRatings(winnerRating, loserRating);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*must be non-negative*");
        }

        #endregion
    }
}
