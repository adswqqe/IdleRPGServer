using FluentAssertions;
using IdleRPG.Application.DTOs.Pet;
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
    /// PetService 단위 테스트
    ///
    /// 테스트 범위:
    /// - DrawPetsAsync: 가챠 로직, 중복 처리, 천장 시스템
    /// - LevelUpPetAsync: 레벨업 비용, 스탯 계산, 소유권 검증
    /// - EquipPetAsync: 장착 로직, 슬롯 교체, 버프 계산
    /// - UnequipPetAsync: 해제 로직
    /// - DeletePetAsync: 장착 중 삭제 불가
    /// </summary>
    public class PetServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICharacterRepository> _mockCharacterRepository;
        private readonly Mock<IPetRepository> _mockPetRepository;
        private readonly Mock<IPetTemplateRepository> _mockPetTemplateRepository;
        private readonly Mock<IEquippedPetsRepository> _mockEquippedPetsRepository;
        private readonly Mock<IRandomProvider> _mockRandomProvider;
        private readonly PetGachaService _petGachaService;
        private readonly Mock<ILogger<PetService>> _mockLogger;
        private readonly PetService _service;

        public PetServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCharacterRepository = new Mock<ICharacterRepository>();
            _mockPetRepository = new Mock<IPetRepository>();
            _mockPetTemplateRepository = new Mock<IPetTemplateRepository>();
            _mockEquippedPetsRepository = new Mock<IEquippedPetsRepository>();
            _mockRandomProvider = new Mock<IRandomProvider>();
            _mockLogger = new Mock<ILogger<PetService>>();

            // PetGachaService는 실제 인스턴스 사용 (Domain Service)
            _petGachaService = new PetGachaService();

            // UnitOfWork Repository 설정
            _mockUnitOfWork.Setup(u => u.Characters).Returns(_mockCharacterRepository.Object);
            _mockUnitOfWork.Setup(u => u.Pets).Returns(_mockPetRepository.Object);
            _mockUnitOfWork.Setup(u => u.PetTemplates).Returns(_mockPetTemplateRepository.Object);
            _mockUnitOfWork.Setup(u => u.EquippedPets).Returns(_mockEquippedPetsRepository.Object);

            _service = new PetService(
                _mockUnitOfWork.Object,
                _petGachaService,
                _mockRandomProvider.Object,
                _mockLogger.Object
            );
        }

        #region DrawPetsAsync - Validation Tests

        /// <summary>
        /// 크리스탈 부족 시 InvalidOperationException 발생
        /// </summary>
        [Fact]
        public async Task DrawPetsAsync_InsufficientCrystal_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 50, // 100 미만
                PetGachaCount = 0
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Act
            Func<Task> act = async () => await _service.DrawPetsAsync(characterId, count: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Crystal이 부족합니다*");
        }

        /// <summary>
        /// 잘못된 count 값 (1 또는 10이 아닌 경우) → InvalidOperationException
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(11)]
        public async Task DrawPetsAsync_InvalidCount_ThrowsInvalidOperationException(int invalidCount)
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 1000,
                PetGachaCount = 0
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Act
            Func<Task> act = async () => await _service.DrawPetsAsync(characterId, count: invalidCount);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*가챠 횟수는 1 또는 10만 가능합니다*");
        }

        #endregion

        #region DrawPetsAsync - Success Cases

        /// <summary>
        /// 단건 가챠 (count=1): 크리스탈 100 소모, 신규 펫 획득
        /// </summary>
        [Fact]
        public async Task DrawPetsAsync_SingleGacha_DeductsCrystalAndReturnsPet()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                PetGachaCount = 0
            };

            var template = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Random = 60 → Common (40-99 범위)
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(60);

            _mockPetTemplateRepository.Setup(r => r.GetByRarityAsync(Rarity.Common, default))
                .ReturnsAsync(new List<PetTemplate> { template });

            // 랜덤 템플릿 선택 (0번째)
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            // 중복 체크: 없음
            _mockPetRepository.Setup(r => r.GetDuplicateAsync(characterId, 1, default))
                .ReturnsAsync((Pet?)null);

            // Act
            var result = await _service.DrawPetsAsync(characterId, count: 1);

            // Assert
            result.Should().NotBeNull();
            result.Pets.Should().HaveCount(1);
            result.Pets[0].TemplateName.Should().Be("Slime");
            result.RemainingCrystal.Should().Be(100); // 200 - 100
            result.CurrentPityCount.Should().Be(1); // 0 + 1
            result.DuplicateRewards.Should().BeEmpty();
            result.TotalGoldFromDuplicates.Should().Be(0);

            // Repository 호출 검증
            _mockPetRepository.Verify(r => r.AddAsync(It.IsAny<Pet>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// 10연차 가챠 (count=10): 크리스탈 900 소모, pityCount는 Soft Pity 미도달
        /// </summary>
        [Fact]
        public async Task DrawPetsAsync_TenGacha_DeductsCrystal900()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 1000,
                PetGachaCount = 0
            };

            var commonTemplate = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // 10번 모두 Common (60 반환)
            // NOTE: pityCount가 0→9까지는 Soft Pity 미발동, 모두 Common
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(60);
            _mockRandomProvider.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

            // Common 템플릿 Mock만 설정 (rand=60은 항상 Common 반환)
            _mockPetTemplateRepository.Setup(r => r.GetByRarityAsync(Rarity.Common, default))
                .ReturnsAsync(new List<PetTemplate> { commonTemplate });

            _mockPetRepository.Setup(r => r.GetDuplicateAsync(characterId, 1, default))
                .ReturnsAsync((Pet?)null);

            // Act
            var result = await _service.DrawPetsAsync(characterId, count: 10);

            // Assert
            result.Pets.Should().HaveCount(10);
            result.RemainingCrystal.Should().Be(100); // 1000 - 900
            result.CurrentPityCount.Should().Be(10); // 0 + 10 (Legendary 없으므로 초기화 안됨)

            _mockPetRepository.Verify(r => r.AddAsync(It.IsAny<Pet>(), default), Times.Exactly(10));
        }

        #endregion

        #region DrawPetsAsync - Duplicate Handling

        /// <summary>
        /// 중복 펫 획득 시 골드 보상 (Common: 100 골드)
        /// </summary>
        [Fact]
        public async Task DrawPetsAsync_DuplicatePet_ReturnsGoldReward()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                Gold = 1000,
                PetGachaCount = 0
            };

            var template = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            var existingPet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockRandomProvider.Setup(r => r.Next(100)).Returns(60); // Common
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            _mockPetTemplateRepository.Setup(r => r.GetByRarityAsync(Rarity.Common, default))
                .ReturnsAsync(new List<PetTemplate> { template });

            // 중복 체크: 존재
            _mockPetRepository.Setup(r => r.GetDuplicateAsync(characterId, 1, default))
                .ReturnsAsync(existingPet);

            // Act
            var result = await _service.DrawPetsAsync(characterId, count: 1);

            // Assert
            result.Pets.Should().BeEmpty(); // 신규 펫 없음
            result.DuplicateRewards.Should().HaveCount(1);
            result.DuplicateRewards[0].PetTemplateName.Should().Be("Slime");
            result.DuplicateRewards[0].GoldReward.Should().Be(100); // Common = 100 골드
            result.TotalGoldFromDuplicates.Should().Be(100);
            character.Gold.Should().Be(1100); // 1000 + 100

            // 신규 펫 추가 없음
            _mockPetRepository.Verify(r => r.AddAsync(It.IsAny<Pet>(), default), Times.Never);
        }

        /// <summary>
        /// 중복 펫 희귀도별 골드 보상 검증
        /// Common: 100, Rare: 500, Epic: 2000, Legendary: 10000
        /// </summary>
        [Theory]
        [InlineData(Rarity.Common, 100)]
        [InlineData(Rarity.Rare, 500)]
        [InlineData(Rarity.Epic, 2000)]
        [InlineData(Rarity.Legendary, 10000)]
        public async Task DrawPetsAsync_DuplicatePet_GoldRewardByRarity(Rarity rarity, int expectedGold)
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                Gold = 0,
                PetGachaCount = 0
            };

            var template = new PetTemplate
            {
                Id = 1,
                Name = "TestPet",
                Rarity = rarity,
                BaseAttack = 50,
                BaseMana = 25
            };

            var existingPet = new Pet { Id = 1, CharacterId = characterId, TemplateId = 1, Level = 1 };

            // Soft Pity 활용: pityCount=45 → adjustedRate=7 → rand=5 → Legendary
            if (rarity == Rarity.Legendary)
            {
                character.PetGachaCount = 45; // Soft Pity 구간
                _mockRandomProvider.Setup(r => r.Next(100)).Returns(5); // [0,7) → Legendary
            }
            else
            {
                // 다른 Rarity는 기본 확률 사용
                int randValue = rarity switch
                {
                    Rarity.Epic => 5,      // [1,10) → Epic
                    Rarity.Rare => 20,     // [10,40) → Rare
                    Rarity.Common => 60,   // [40,100) → Common
                    _ => 60
                };
                _mockRandomProvider.Setup(r => r.Next(100)).Returns(randValue);
            }

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            _mockPetTemplateRepository.Setup(r => r.GetByRarityAsync(rarity, default))
                .ReturnsAsync(new List<PetTemplate> { template });

            _mockPetRepository.Setup(r => r.GetDuplicateAsync(characterId, 1, default))
                .ReturnsAsync(existingPet);

            // Act
            var result = await _service.DrawPetsAsync(characterId, count: 1);

            // Assert
            result.DuplicateRewards[0].GoldReward.Should().Be(expectedGold);
            character.Gold.Should().Be(expectedGold);
        }

        #endregion

        #region DrawPetsAsync - Pity System

        /// <summary>
        /// Legendary 획득 시 PetGachaCount 초기화 (0으로)
        /// </summary>
        [Fact]
        public async Task DrawPetsAsync_LegendaryDrawn_ResetsPityCount()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Crystal = 200,
                PetGachaCount = 45 // Soft Pity 구간
            };

            var template = new PetTemplate
            {
                Id = 11,
                Name = "Ancient Guardian",
                Rarity = Rarity.Legendary,
                BaseAttack = 350,
                BaseMana = 200
            };

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Soft Pity 45 → adjustedRate = 7 → Random 5 → Legendary
            _mockRandomProvider.Setup(r => r.Next(100)).Returns(5);
            _mockRandomProvider.Setup(r => r.Next(1)).Returns(0);

            _mockPetTemplateRepository.Setup(r => r.GetByRarityAsync(Rarity.Legendary, default))
                .ReturnsAsync(new List<PetTemplate> { template });

            _mockPetRepository.Setup(r => r.GetDuplicateAsync(characterId, 11, default))
                .ReturnsAsync((Pet?)null);

            // Act
            var result = await _service.DrawPetsAsync(characterId, count: 1);

            // Assert
            result.Pets[0].RarityName.Should().Be("Legendary");
            result.CurrentPityCount.Should().Be(0); // 45 → 0 (초기화)
            character.PetGachaCount.Should().Be(0);
        }

        #endregion

        #region LevelUpPetAsync - Validation Tests

        /// <summary>
        /// 최대 레벨(50) 펫 레벨업 시도 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task LevelUpPetAsync_MaxLevel_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 50, // Max level
                CurrentAttack = 500,
                CurrentMana = 250
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // Act
            Func<Task> act = async () => await _service.LevelUpPetAsync(1, characterId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*최대 레벨입니다*");
        }

        /// <summary>
        /// 골드 부족 시 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task LevelUpPetAsync_InsufficientGold_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Gold = 50 // 레벨업 비용(100) 미만
            };

            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1,
                CurrentAttack = 50,
                CurrentMana = 25
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            // Act
            Func<Task> act = async () => await _service.LevelUpPetAsync(1, characterId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*골드가 부족합니다*");
        }

        /// <summary>
        /// 다른 캐릭터의 펫 레벨업 시도 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task LevelUpPetAsync_NotOwner_ThrowsInvalidOperationException()
        {
            // Arrange
            var ownerCharacterId = Guid.NewGuid();
            var otherCharacterId = Guid.NewGuid();

            var pet = new Pet
            {
                Id = 1,
                CharacterId = ownerCharacterId, // 다른 캐릭터 소유
                TemplateId = 1,
                Level = 1
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // Act
            Func<Task> act = async () => await _service.LevelUpPetAsync(1, otherCharacterId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*권한이 없습니다*");
        }

        #endregion

        #region LevelUpPetAsync - Success Cases

        /// <summary>
        /// 레벨업 성공: 스탯 증가 (Attack +10, Mana +5), 골드 차감
        /// </summary>
        [Fact]
        public async Task LevelUpPetAsync_Success_UpdatesStats()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Gold = 500
            };

            var template = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1,
                CurrentAttack = 50, // BaseAttack
                CurrentMana = 25,   // BaseMana
                PetTemplate = template
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockPetTemplateRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(template);

            // Act
            var result = await _service.LevelUpPetAsync(1, characterId);

            // Assert
            result.NewLevel.Should().Be(2); // 1 → 2
            result.NewAttack.Should().Be(60); // BaseAttack(50) + (Level-1)*10 = 50 + 10
            result.NewMana.Should().Be(30);   // BaseMana(25) + (Level-1)*5 = 25 + 5
            result.CostGold.Should().Be(100); // 100 * (1.5 ^ (1-1)) = 100
            result.RemainingGold.Should().Be(400); // 500 - 100

            pet.Level.Should().Be(2);
            pet.CurrentAttack.Should().Be(60);
            pet.CurrentMana.Should().Be(30);
            character.Gold.Should().Be(400);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// 레벨업 비용 계산: 100 * (1.5 ^ (Level-1))
        /// Level 1→2: 100, Level 2→3: 150, Level 3→4: 225
        /// </summary>
        [Theory]
        [InlineData(1, 100)]    // 100 * (1.5^0) = 100
        [InlineData(2, 150)]    // 100 * (1.5^1) = 150
        [InlineData(3, 225)]    // 100 * (1.5^2) = 225
        [InlineData(5, 506)]    // 100 * (1.5^4) = 506.25 → 506
        public async Task LevelUpPetAsync_CostCalculation_IsCorrect(int currentLevel, int expectedCost)
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var character = new Character
            {
                Id = characterId,
                Gold = 10000 // 충분한 골드
            };

            var template = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = currentLevel,
                CurrentAttack = 50 + (currentLevel - 1) * 10,
                CurrentMana = 25 + (currentLevel - 1) * 5,
                PetTemplate = template
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            _mockCharacterRepository.Setup(r => r.GetByIdAsync(characterId))
                .ReturnsAsync(character);

            _mockPetTemplateRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(template);

            // Act
            var result = await _service.LevelUpPetAsync(1, characterId);

            // Assert
            result.CostGold.Should().Be(expectedCost);
        }

        #endregion

        #region EquipPetAsync - Validation Tests

        /// <summary>
        /// 잘못된 슬롯 인덱스 (1-3 범위 밖) → InvalidOperationException
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(-1)]
        public async Task EquipPetAsync_InvalidSlot_ThrowsInvalidOperationException(int invalidSlot)
        {
            // Arrange
            var characterId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _service.EquipPetAsync(characterId, petId: 1, slotIndex: invalidSlot);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*슬롯 인덱스는 1-3 범위여야 합니다*");
        }

        /// <summary>
        /// 다른 캐릭터의 펫 장착 시도 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task EquipPetAsync_NotOwner_ThrowsInvalidOperationException()
        {
            // Arrange
            var ownerCharacterId = Guid.NewGuid();
            var otherCharacterId = Guid.NewGuid();

            var pet = new Pet
            {
                Id = 1,
                CharacterId = ownerCharacterId, // 다른 캐릭터 소유
                TemplateId = 1,
                Level = 1
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // Act
            Func<Task> act = async () => await _service.EquipPetAsync(otherCharacterId, petId: 1, slotIndex: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*권한이 없습니다*");
        }

        #endregion

        #region EquipPetAsync - Success Cases

        /// <summary>
        /// 빈 슬롯에 펫 장착 성공
        /// </summary>
        [Fact]
        public async Task EquipPetAsync_EmptySlot_EquipsPet()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var template = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 5,
                CurrentAttack = 90,
                CurrentMana = 45,
                PetTemplate = template
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // 중복 장착 체크: 없음
            _mockEquippedPetsRepository.Setup(r => r.GetByPetIdAsync(1, default))
                .ReturnsAsync((EquippedPets?)null);

            // 슬롯 점유 체크: 없음
            _mockEquippedPetsRepository.Setup(r => r.GetBySlotAsync(characterId, 1, default))
                .ReturnsAsync((EquippedPets?)null);

            // 장착된 펫 조회 (버프 계산용)
            _mockEquippedPetsRepository.Setup(r => r.GetByCharacterIdAsync(characterId, default))
                .ReturnsAsync(new List<EquippedPets>
                {
                    new EquippedPets
                    {
                        CharacterId = characterId,
                        SlotIndex = 1,
                        PetId = 1,
                        Pet = pet
                    }
                });

            // Act
            var result = await _service.EquipPetAsync(characterId, petId: 1, slotIndex: 1);

            // Assert
            result.EquippedPets.Should().HaveCount(1);
            result.EquippedPets[0].SlotIndex.Should().Be(1);
            result.EquippedPets[0].PetName.Should().Be("Slime");
            result.EquippedPets[0].BuffAttack.Should().Be(9); // 90 * 0.1 = 9
            result.EquippedPets[0].BuffMana.Should().Be(4);   // 45 * 0.1 = 4 (소수점 버림)
            result.TotalBuffAttack.Should().Be(9);
            result.TotalBuffMana.Should().Be(4);

            _mockEquippedPetsRepository.Verify(r => r.AddAsync(It.IsAny<EquippedPets>(), default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// 슬롯 점유 시 기존 펫 교체
        /// </summary>
        [Fact]
        public async Task EquipPetAsync_SlotOccupied_ReplacesExistingPet()
        {
            // Arrange
            var characterId = Guid.NewGuid();

            var oldTemplate = new PetTemplate
            {
                Id = 1,
                Name = "Slime",
                Rarity = Rarity.Common,
                BaseAttack = 50,
                BaseMana = 25
            };

            var newTemplate = new PetTemplate
            {
                Id = 2,
                Name = "Wolf",
                Rarity = Rarity.Common,
                BaseAttack = 60,
                BaseMana = 20
            };

            var oldPet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1,
                CurrentAttack = 50,
                CurrentMana = 25,
                PetTemplate = oldTemplate
            };

            var newPet = new Pet
            {
                Id = 2,
                CharacterId = characterId,
                TemplateId = 2,
                Level = 1,
                CurrentAttack = 60,
                CurrentMana = 20,
                PetTemplate = newTemplate
            };

            var oldEquippedPet = new EquippedPets
            {
                CharacterId = characterId,
                SlotIndex = 1,
                PetId = 1,
                Pet = oldPet
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(2, default))
                .ReturnsAsync(newPet);

            // 중복 장착 체크: 없음 (새 펫은 미장착)
            _mockEquippedPetsRepository.Setup(r => r.GetByPetIdAsync(2, default))
                .ReturnsAsync((EquippedPets?)null);

            // 슬롯 점유 체크: 기존 펫 있음
            _mockEquippedPetsRepository.Setup(r => r.GetBySlotAsync(characterId, 1, default))
                .ReturnsAsync(oldEquippedPet);

            // 장착된 펫 조회 (버프 계산용)
            _mockEquippedPetsRepository.Setup(r => r.GetByCharacterIdAsync(characterId, default))
                .ReturnsAsync(new List<EquippedPets>
                {
                    new EquippedPets
                    {
                        CharacterId = characterId,
                        SlotIndex = 1,
                        PetId = 2,
                        Pet = newPet
                    }
                });

            // Act
            var result = await _service.EquipPetAsync(characterId, petId: 2, slotIndex: 1);

            // Assert
            result.EquippedPets[0].PetName.Should().Be("Wolf");
            result.TotalBuffAttack.Should().Be(6); // 60 * 0.1

            // 기존 펫 삭제 → 새 펫 추가
            _mockEquippedPetsRepository.Verify(r => r.DeleteAsync(oldEquippedPet, default), Times.Once);
            _mockEquippedPetsRepository.Verify(r => r.AddAsync(It.IsAny<EquippedPets>(), default), Times.Once);
        }

        #endregion

        #region UnequipPetAsync - Tests

        /// <summary>
        /// 펫 해제 성공
        /// </summary>
        [Fact]
        public async Task UnequipPetAsync_Success_RemovesPet()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var equippedPet = new EquippedPets
            {
                CharacterId = characterId,
                SlotIndex = 1,
                PetId = 1
            };

            _mockEquippedPetsRepository.Setup(r => r.GetBySlotAsync(characterId, 1, default))
                .ReturnsAsync(equippedPet);

            // Act
            await _service.UnequipPetAsync(characterId, slotIndex: 1);

            // Assert
            _mockEquippedPetsRepository.Verify(r => r.DeleteAsync(equippedPet, default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        /// <summary>
        /// 빈 슬롯 해제 시도 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task UnequipPetAsync_EmptySlot_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();

            _mockEquippedPetsRepository.Setup(r => r.GetBySlotAsync(characterId, 1, default))
                .ReturnsAsync((EquippedPets?)null);

            // Act
            Func<Task> act = async () => await _service.UnequipPetAsync(characterId, slotIndex: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*장착된 펫이 없습니다*");
        }

        #endregion

        #region DeletePetAsync - Tests

        /// <summary>
        /// 장착 중인 펫 삭제 시도 → InvalidOperationException
        /// </summary>
        [Fact]
        public async Task DeletePetAsync_EquippedPet_ThrowsInvalidOperationException()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1
            };

            var equippedPet = new EquippedPets
            {
                CharacterId = characterId,
                SlotIndex = 1,
                PetId = 1
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // 장착 중
            _mockEquippedPetsRepository.Setup(r => r.GetByPetIdAsync(1, default))
                .ReturnsAsync(equippedPet);

            // Act
            Func<Task> act = async () => await _service.DeletePetAsync(1, characterId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*장착 중인 펫은 삭제할 수 없습니다*");
        }

        /// <summary>
        /// 미장착 펫 삭제 성공
        /// </summary>
        [Fact]
        public async Task DeletePetAsync_UnequippedPet_DeletesSuccessfully()
        {
            // Arrange
            var characterId = Guid.NewGuid();
            var pet = new Pet
            {
                Id = 1,
                CharacterId = characterId,
                TemplateId = 1,
                Level = 1
            };

            _mockPetRepository.Setup(r => r.GetByIdAsync(1, default))
                .ReturnsAsync(pet);

            // 미장착
            _mockEquippedPetsRepository.Setup(r => r.GetByPetIdAsync(1, default))
                .ReturnsAsync((EquippedPets?)null);

            // Act
            await _service.DeletePetAsync(1, characterId);

            // Assert
            _mockPetRepository.Verify(r => r.DeleteAsync(pet, default), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        #endregion
    }
}
