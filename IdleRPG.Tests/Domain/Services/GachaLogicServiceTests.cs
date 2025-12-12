using FluentAssertions;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using Moq;
using Xunit;

namespace IdleRPG.Tests.Domain.Services
{
    public class GachaLogicServiceTests
    {
        private readonly Mock<IRandomProvider> _mockRandomProvider;
        private readonly GachaLogicService _service;

        public GachaLogicServiceTests()
        {
            _mockRandomProvider = new Mock<IRandomProvider>();
            _service = new GachaLogicService(_mockRandomProvider.Object);
        }

        #region DetermineRarity Tests

        [Fact]
        public void DetermineRarity_PityCount100OrMore_ReturnsLegendary()
        {
            // Arrange
            int pityCount = 100;

            // Act
            var result = _service.DetermineRarity(pityCount);

            // Assert
            result.Should().Be(SkillRarity.Legendary);
        }

        [Fact]
        public void DetermineRarity_RandomValue0_ReturnsLegendary()
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(0);

            // Act
            var result = _service.DetermineRarity(0);

            // Assert
            result.Should().Be(SkillRarity.Legendary);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void DetermineRarity_RandomValue1To9_ReturnsEpic(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DetermineRarity(0);

            // Assert
            result.Should().Be(SkillRarity.Epic);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(39)]
        public void DetermineRarity_RandomValue10To39_ReturnsRare(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DetermineRarity(0);

            // Assert
            result.Should().Be(SkillRarity.Rare);
        }

        [Theory]
        [InlineData(40)]
        [InlineData(70)]
        [InlineData(99)]
        public void DetermineRarity_RandomValue40To99_ReturnsCommon(int randomValue)
        {
            // Arrange
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(randomValue);

            // Act
            var result = _service.DetermineRarity(0);

            // Assert
            result.Should().Be(SkillRarity.Common);
        }

        #endregion

        #region SelectRandomSkill Tests

        [Fact]
        public void SelectRandomSkill_WithMatchingRarity_ReturnsCorrectSkill()
        {
            // Arrange
            var skills = new List<SkillTemplate>
            {
                new() { Id = 1, Name = "Common Skill 1", Rarity = SkillRarity.Common },
                new() { Id = 2, Name = "Common Skill 2", Rarity = SkillRarity.Common },
                new() { Id = 3, Name = "Rare Skill", Rarity = SkillRarity.Rare }
            };

            _mockRandomProvider.Setup(r => r.Next(2)).Returns(0);

            // Act
            var result = _service.SelectRandomSkill(SkillRarity.Common, skills);

            // Assert
            result.Should().NotBeNull();
            result.Rarity.Should().Be(SkillRarity.Common);
        }

        [Fact]
        public void SelectRandomSkill_WithNoMatchingRarity_ThrowsArgumentException()
        {
            // Arrange
            var skills = new List<SkillTemplate>
            {
                new() { Id = 1, Name = "Common Skill", Rarity = SkillRarity.Common }
            };

            // Act
            Action act = () => _service.SelectRandomSkill(SkillRarity.Legendary, skills);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*'Legendary' 등급의 스킬이 없습니다*");
        }

        [Fact]
        public void SelectRandomSkill_WithEmptyList_ThrowsArgumentException()
        {
            // Arrange
            var skills = new List<SkillTemplate>();

            // Act
            Action act = () => _service.SelectRandomSkill(SkillRarity.Common, skills);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region GetPityCountAfterDraw Tests

        [Fact]
        public void GetPityCountAfterDraw_LegendaryDrawn_ResetsPityCountToZero()
        {
            // Arrange
            int currentPityCount = 50;

            // Act
            var result = _service.GetPityCountAfterDraw(SkillRarity.Legendary, currentPityCount);

            // Assert
            result.Should().Be(0);
        }

        [Theory]
        [InlineData(SkillRarity.Common)]
        [InlineData(SkillRarity.Rare)]
        [InlineData(SkillRarity.Epic)]
        public void GetPityCountAfterDraw_NonLegendaryDrawn_IncrementsCount(SkillRarity rarity)
        {
            // Arrange
            int currentPityCount = 50;

            // Act
            var result = _service.GetPityCountAfterDraw(rarity, currentPityCount);

            // Assert
            result.Should().Be(51);
        }

        #endregion
    }
}
