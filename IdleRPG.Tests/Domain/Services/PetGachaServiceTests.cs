using FluentAssertions;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using Moq;
using Xunit;

namespace IdleRPG.Tests.Domain.Services
{
    /// <summary>
    /// PetGachaService 단위 테스트
    ///
    /// 테스트 범위:
    /// - Hard Pity (50회): Legendary 보장
    /// - Soft Pity (40-49회): Legendary 확률 증가 (1% → 2-11%)
    /// - 기본 확률: Legendary 1%, Epic 9%, Rare 30%, Common 60%
    /// - Pity 카운터 관리: Legendary 획득 시 0 초기화
    /// - Input Validation: pityCount 범위 검증 (0-50)
    /// </summary>
    public class PetGachaServiceTests
    {
        private readonly Mock<IRandomProvider> _mockRandomProvider;
        private readonly PetGachaService _service;

        public PetGachaServiceTests()
        {
            _mockRandomProvider = new Mock<IRandomProvider>();
            _service = new PetGachaService();
        }

        #region DrawPet Tests - Hard Pity

        /// <summary>
        /// Hard Pity: pityCount가 50 이상이면 Legendary 보장
        /// </summary>
        [Fact]
        public void DrawPet_PityCount50_ReturnsLegendary()
        {
            // Arrange
            int pityCount = 50;

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "Hard Pity at 50 guarantees Legendary");
        }

        /// <summary>
        /// Hard Pity: pityCount가 50 이상이면 Random 호출 없이 Legendary 반환
        /// </summary>
        [Fact]
        public void DrawPet_PityCount50_DoesNotCallRandom()
        {
            // Arrange
            int pityCount = 50;

            // Act
            _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            _mockRandomProvider.Verify(r => r.Next(It.IsAny<int>()), Times.Never,
                "Hard Pity should not call random number generator");
        }

        #endregion

        #region DrawPet Tests - Soft Pity

        /// <summary>
        /// Soft Pity: pityCount가 40이면 Legendary 확률 1% → 2%
        /// adjustedRate = 1 + (40 - 39) = 2
        /// </summary>
        [Fact]
        public void DrawPet_PityCount40_IncreasesLegendaryRate()
        {
            // Arrange
            int pityCount = 40;
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(0); // [0, 2) → Legendary (2% 확률)

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "Soft Pity at 40 increases Legendary rate to 2%");
        }

        /// <summary>
        /// Soft Pity: pityCount가 45이면 Legendary 확률 1% → 7%
        /// adjustedRate = 1 + (45 - 39) = 7
        /// </summary>
        [Fact]
        public void DrawPet_PityCount45_IncreasesLegendaryRateToSeven()
        {
            // Arrange
            int pityCount = 45;
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(5); // [0, 7) → Legendary (7% 확률)

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "Soft Pity at 45 increases Legendary rate to 7%");
        }

        /// <summary>
        /// Soft Pity: pityCount가 49이면 Legendary 확률 1% → 11%
        /// adjustedRate = 1 + (49 - 39) = 11
        /// </summary>
        [Fact]
        public void DrawPet_PityCount49_IncreasesLegendaryRateToEleven()
        {
            // Arrange
            int pityCount = 49;
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(10); // [0, 11) → Legendary (11% 확률)

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "Soft Pity at 49 increases Legendary rate to 11%");
        }

        #endregion

        #region DrawPet Tests - Basic Probability (pityCount = 0)

        /// <summary>
        /// 기본 확률: rand = 0 → Legendary (1%)
        /// </summary>
        [Fact]
        public void DrawPet_PityCount0_RandomValue0_ReturnsLegendary()
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(0);

            // Act
            var result = _service.DrawPet(0, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "Random value 0 falls in [0, 1) → Legendary (1%)");
        }

        /// <summary>
        /// 기본 확률: rand ∈ [1, 9] → Epic (9%)
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void DrawPet_PityCount0_RandomValue1to9_ReturnsEpic(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DrawPet(0, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Epic, $"Random value {randomValue} falls in [1, 10) → Epic (9%)");
        }

        /// <summary>
        /// 기본 확률: rand ∈ [10, 39] → Rare (30%)
        /// </summary>
        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(39)]
        public void DrawPet_PityCount0_RandomValue10to39_ReturnsRare(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DrawPet(0, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Rare, $"Random value {randomValue} falls in [10, 40) → Rare (30%)");
        }

        /// <summary>
        /// 기본 확률: rand ∈ [40, 99] → Common (60%)
        /// </summary>
        [Theory]
        [InlineData(40)]
        [InlineData(70)]
        [InlineData(99)]
        public void DrawPet_PityCount0_RandomValue40to99_ReturnsCommon(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DrawPet(0, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Common, $"Random value {randomValue} falls in [40, 100) → Common (60%)");
        }

        #endregion

        #region DrawPet Tests - Edge Cases

        /// <summary>
        /// Soft Pity 경계: pityCount = 39 (Soft Pity 미적용)
        /// adjustedRate = 1 (기본 확률)
        /// </summary>
        [Fact]
        public void DrawPet_PityCount39_UsesBasicRate()
        {
            // Arrange
            int pityCount = 39;
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(0); // [0, 1) → Legendary (1%)

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "PityCount 39 should use basic 1% rate (Soft Pity starts at 40)");
        }

        /// <summary>
        /// Soft Pity 경계: pityCount = 40 (Soft Pity 적용 시작)
        /// adjustedRate = 2 (1% → 2%)
        /// </summary>
        [Fact]
        public void DrawPet_PityCount40_AppliesSoftPity()
        {
            // Arrange
            int pityCount = 40;
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(1); // [0, 2) → Legendary (2%)

            // Act
            var result = _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            result.Should().Be(Rarity.Legendary, "PityCount 40 should apply Soft Pity (2%)");
        }

        #endregion

        #region GetPityCountAfterDraw Tests

        /// <summary>
        /// Legendary 획득 시 천장 카운터 0 초기화
        /// </summary>
        [Fact]
        public void GetPityCountAfterDraw_LegendaryDrawn_ResetsPityCountToZero()
        {
            // Arrange
            int currentPityCount = 45;

            // Act
            var result = _service.GetPityCountAfterDraw(Rarity.Legendary, currentPityCount);

            // Assert
            result.Should().Be(0, "Legendary draw should reset pity count to 0");
        }

        /// <summary>
        /// Non-Legendary 획득 시 천장 카운터 +1
        /// </summary>
        [Theory]
        [InlineData(Rarity.Common)]
        [InlineData(Rarity.Rare)]
        [InlineData(Rarity.Epic)]
        public void GetPityCountAfterDraw_NonLegendaryDrawn_IncrementsCount(Rarity rarity)
        {
            // Arrange
            int currentPityCount = 30;

            // Act
            var result = _service.GetPityCountAfterDraw(rarity, currentPityCount);

            // Assert
            result.Should().Be(31, $"Drawing {rarity} should increment pity count by 1");
        }

        #endregion

        #region Input Validation Tests

        /// <summary>
        /// pityCount가 음수이면 ArgumentOutOfRangeException 발생
        /// </summary>
        [Fact]
        public void DrawPet_NegativePityCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int pityCount = -1;

            // Act
            Action act = () => _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*pityCount*")
                .And.ParamName.Should().Be("pityCount");
        }

        /// <summary>
        /// pityCount가 50 초과이면 ArgumentOutOfRangeException 발생
        /// </summary>
        [Fact]
        public void DrawPet_PityCountGreaterThan50_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int pityCount = 51;

            // Act
            Action act = () => _service.DrawPet(pityCount, _mockRandomProvider.Object);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*pityCount*")
                .And.ParamName.Should().Be("pityCount");
        }

        /// <summary>
        /// randomProvider가 null이면 ArgumentNullException 발생
        /// </summary>
        [Fact]
        public void DrawPet_NullRandomProvider_ThrowsArgumentNullException()
        {
            // Arrange
            int pityCount = 0;

            // Act
#pragma warning disable CS8625 // null 리터럴을 null로 처리할 수 없는 참조 형식으로 변환할 수 없습니다.
            Action act = () => _service.DrawPet(pityCount, null);
#pragma warning restore CS8625

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*randomProvider*")
                .And.ParamName.Should().Be("randomProvider");
        }

        #endregion
    }
}
