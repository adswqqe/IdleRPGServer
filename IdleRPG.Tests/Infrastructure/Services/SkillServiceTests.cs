using FluentAssertions;
using IdleRPG.Application.DTOs.Gacha;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.Services;
using IdleRPG.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdleRPG.Tests.Infrastructure.Services
{
    /// <summary>
    /// SkillService 테스트
    /// 통합 시나리오: Crystal 검증, 가챠 로직, DB 저장, 에러 처리
    /// </summary>
    public class SkillServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRandomProvider> _mockRandomProvider;
        private readonly GachaLogicService _gachaLogicService;
        private readonly Mock<ILogger<SkillService>> _mockLogger;
        private readonly Mock<ICharacterRepository> _mockCharacterRepository;
        private readonly Mock<ISkillTemplateRepository> _mockSkillTemplateRepository;
        private readonly Mock<ICharacterSkillRepository> _mockCharacterSkillRepository;
        private readonly SkillService _service;

        public SkillServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCharacterRepository = new Mock<ICharacterRepository>();
            _mockSkillTemplateRepository = new Mock<ISkillTemplateRepository>();
            _mockCharacterSkillRepository = new Mock<ICharacterSkillRepository>();
            _mockLogger = new Mock<ILogger<SkillService>>();

            // GachaLogicService는 실제 인스턴스 사용 (Domain Service)
            _mockRandomProvider = new Mock<IRandomProvider>();
            _gachaLogicService = new GachaLogicService(_mockRandomProvider.Object);

            // UnitOfWork Repository 설정
            _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
            _mockUnitOfWork.Setup(u => u.SkillTemplates).Returns(_mockSkillTemplateRepository.Object);
            _mockUnitOfWork.Setup(u => u.CharacterSkills).Returns(_mockCharacterSkillRepository.Object);

            _service = new SkillService(
                _mockUnitOfWork.Object,
                _gachaLogicService,
                _mockLogger.Object
            );
        }

        #region PerformGachaAsync - Success Cases

        [Fact]
        public async Task PerformGachaAsync_ValidRequest_ReturnsSkillDto()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                GachaPityCount = 50
            };

            var skillTemplate = new SkillTemplate
            {
                Id = 1,
                Name = "메테오",
                Description = "강력한 공격",
                Rarity = SkillRarity.Epic,
                Type = SkillType.Active
            };

            var allSkills = new List<SkillTemplate> { skillTemplate };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allSkills);

            // Random = 5 → Epic (1-9 범위)
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(5);
            // Epic 스킬 중 첫 번째 선택
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            var result = await _service.PerformGachaAsync(characterId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("메테오");
            result.Rarity.Should().Be(SkillRarity.Epic);
            result.Type.Should().Be(SkillType.Active);
        }

        [Fact]
        public async Task PerformGachaAsync_DeductsCrystalCorrectly()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 150,
                GachaPityCount = 0
            };

            var skillTemplate = new SkillTemplate
            {
                Id = 1,
                Name = "화염구",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SkillTemplate> { skillTemplate });

            // Random = 60 → Common (40-99 범위)
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(60);
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            await _service.PerformGachaAsync(characterId, gachaCost: 100);

            // Assert
            character.Crystal.Should().Be(50); // 150 - 100 = 50
        }

        [Fact]
        public async Task PerformGachaAsync_SavesCharacterSkillToDatabase()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                GachaPityCount = 0
            };

            var skillTemplate = new SkillTemplate
            {
                Id = 5,
                Name = "치유",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SkillTemplate> { skillTemplate });

            // Random = 80 → Common
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(80);
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            await _service.PerformGachaAsync(characterId);

            // Assert
            _mockCharacterSkillRepository.Verify(r => r.AddAsync(It.Is<CharacterSkill>(
                cs => cs.CharacterId == characterId &&
                      cs.SkillTemplateId == 5 &&
                      cs.IsEquipped == false
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        #endregion

        #region PerformGachaAsync - Error Cases

        [Fact]
        public async Task PerformGachaAsync_CharacterNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync((Character?)null);

            // Act
            Func<Task> act = async () => await _service.PerformGachaAsync(characterId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"캐릭터를 찾을 수 없습니다. (ID: {characterId})");
        }

        [Fact]
        public async Task PerformGachaAsync_InsufficientCrystal_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 50, // 부족
                GachaPityCount = 0
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Act
            Func<Task> act = async () => await _service.PerformGachaAsync(characterId, gachaCost: 100);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Crystal이 부족합니다. (필요: 100, 보유: 50)");
        }

        [Fact]
        public async Task PerformGachaAsync_ExactCrystalAmount_Succeeds()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 100, // 정확히 필요한 양
                GachaPityCount = 0
            };

            var skillTemplate = new SkillTemplate
            {
                Id = 1,
                Name = "강타",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SkillTemplate> { skillTemplate });

            // Random = 70 → Common
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(70);
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            var result = await _service.PerformGachaAsync(characterId, gachaCost: 100);

            // Assert
            result.Should().NotBeNull();
            character.Crystal.Should().Be(0); // 100 - 100 = 0
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task PerformGachaAsync_PityCount99_IncrementsToPityCount100()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                GachaPityCount = 99 // 천장 직전
            };

            var commonSkill = new SkillTemplate
            {
                Id = 1,
                Name = "화염구",
                Rarity = SkillRarity.Common,
                Type = SkillType.Active
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SkillTemplate> { commonSkill });

            // Random = 70 → Common
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(70);
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            await _service.PerformGachaAsync(characterId);

            // Assert
            // PityCount 99 + 1 = 100 (Common이 나왔으므로 증가)
            character.GachaPityCount.Should().Be(100);
        }

        [Fact]
        public async Task PerformGachaAsync_PityCount100_ForcesLegendaryAndResetsToZero()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                GachaPityCount = 100 // 천장 도달
            };

            var legendarySkill = new SkillTemplate
            {
                Id = 22,
                Name = "신의 심판",
                Rarity = SkillRarity.Legendary,
                Type = SkillType.Active
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockSkillTemplateRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<SkillTemplate> { legendarySkill });

            // 천장 시스템으로 Legendary 강제 (Random 값 무관)
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // Act
            var result = await _service.PerformGachaAsync(characterId);

            // Assert
            result.Rarity.Should().Be(SkillRarity.Legendary);
            character.GachaPityCount.Should().Be(0); // Legendary이므로 리셋
        }

        #endregion
    }
}
