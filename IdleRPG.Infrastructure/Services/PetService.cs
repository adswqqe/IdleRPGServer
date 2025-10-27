using IdleRPG.Application.DTOs.Pet;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Services
{
    /// <summary>
    /// 펫 관련 비즈니스 로직 구현체
    /// </summary>
    public class PetService : IPetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PetGachaService _petGachaService;
        private readonly IRandomProvider _randomProvider;
        private readonly ILogger<PetService> _logger;

        public PetService(
            IUnitOfWork unitOfWork,
            PetGachaService petGachaService,
            IRandomProvider randomProvider,
            ILogger<PetService> logger)
        {
            _unitOfWork = unitOfWork;
            _petGachaService = petGachaService;
            _randomProvider = randomProvider;
            _logger = logger;
        }

        /// <summary>
        /// 펫 가챠를 수행합니다.
        /// 1. 캐릭터 조회 및 Crystal 검증
        /// 2. 현재 천장 카운트 확인
        /// 3. PetGachaService로 희귀도 결정
        /// 4. 해당 희귀도의 펫 템플릿 중 랜덤 선택
        /// 5. 중복 체크 (CharacterId + TemplateId)
        /// 6. 중복 아님 → Pet 생성 / 중복 → 골드 보상
        /// 7. Crystal 차감, 천장 카운트 업데이트
        /// </summary>
        public async Task<PetGachaResponseDto> DrawPetsAsync(Guid characterId, int count, CancellationToken cancellationToken = default)
        {
            // 1. 입력 검증
            if (count != 1 && count != 10)
            {
                _logger.LogWarning("펫 가챠 실패: 잘못된 가챠 횟수. Count={Count}", count);
                throw new InvalidOperationException($"가챠 횟수는 1 또는 10만 가능합니다. (입력: {count})");
            }

            // 2. 캐릭터 조회
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
            {
                _logger.LogWarning("펫 가챠 실패: 캐릭터를 찾을 수 없습니다. CharacterId={CharacterId}", characterId);
                throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (ID: {characterId})");
            }

            // 3. 가챠 비용 계산
            int costPerDraw = 100;
            int totalCost = (count == 1) ? costPerDraw : 900; // 10연차 할인

            // 4. Crystal 검증
            if (character.Crystal < totalCost)
            {
                _logger.LogWarning("펫 가챠 실패: Crystal 부족. 필요={Cost}, 보유={Crystal}", totalCost, character.Crystal);
                throw new InvalidOperationException($"Crystal이 부족합니다. (필요: {totalCost}, 보유: {character.Crystal})");
            }

            // 5. Crystal 차감 (트랜잭션 시작)
            character.Crystal -= totalCost;

            // 6. 가챠 루프 (count 횟수)
            var newPets = new List<PetDto>();
            var duplicateRewards = new List<DuplicateRewardDto>();
            int totalGoldFromDuplicates = 0;

            for (int i = 0; i < count; i++)
            {
                // 6-1. 희귀도 결정 (PetGachaService 사용)
                var rarity = _petGachaService.DrawPet(character.PetGachaCount, _randomProvider);
                _logger.LogInformation("펫 가챠 희귀도 결정: {Rarity}, PityCount={PityCount}", rarity, character.PetGachaCount);

                // 6-2. 해당 희귀도의 펫 템플릿 조회
                var templates = await _unitOfWork.PetTemplates.GetByRarityAsync(rarity, cancellationToken);
                if (templates == null || templates.Count == 0)
                {
                    _logger.LogError("펫 가챠 실패: 해당 희귀도의 템플릿이 없습니다. Rarity={Rarity}", rarity);
                    throw new InvalidOperationException($"해당 희귀도의 펫 템플릿이 없습니다. ({rarity})");
                }

                // 6-3. 랜덤 템플릿 선택
                int randomIndex = _randomProvider.Next(templates.Count);
                var selectedTemplate = templates[randomIndex];

                _logger.LogInformation("펫 가챠 결과: {PetName} ({Rarity})", selectedTemplate.Name, selectedTemplate.Rarity);

                // 6-4. 중복 체크 (CharacterId + TemplateId)
                var duplicatePet = await _unitOfWork.Pets.GetDuplicateAsync(characterId, selectedTemplate.Id, cancellationToken);

                if (duplicatePet != null)
                {
                    // 중복: 골드 보상
                    int goldReward = rarity switch
                    {
                        Rarity.Common => 100,
                        Rarity.Rare => 500,
                        Rarity.Epic => 2000,
                        Rarity.Legendary => 10000,
                        _ => 100
                    };

                    character.Gold += goldReward;
                    totalGoldFromDuplicates += goldReward;

                    duplicateRewards.Add(new DuplicateRewardDto
                    {
                        PetTemplateName = selectedTemplate.Name,
                        GoldReward = goldReward
                    });

                    _logger.LogInformation("펫 중복: {PetName} → 골드 보상 {GoldReward}", selectedTemplate.Name, goldReward);
                }
                else
                {
                    // 중복 아님: Pet Entity 생성
                    var newPet = new Pet
                    {
                        CharacterId = characterId,
                        TemplateId = selectedTemplate.Id,
                        Level = 1,
                        CurrentAttack = selectedTemplate.BaseAttack,
                        CurrentMana = selectedTemplate.BaseMana,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Pets.AddAsync(newPet, cancellationToken);

                    // DTO 변환 (신규 펫만 포함)
                    newPets.Add(new PetDto
                    {
                        Id = newPet.Id, // AddAsync 후 EF Core가 자동 생성 (SaveChanges 후)
                        TemplateName = selectedTemplate.Name,
                        RarityName = selectedTemplate.Rarity.ToString(),
                        Level = newPet.Level,
                        CurrentAttack = newPet.CurrentAttack,
                        CurrentMana = newPet.CurrentMana,
                        ImageUrl = $"/resources/pets/{selectedTemplate.Id}.png",
                        IsEquipped = false
                    });

                    _logger.LogInformation("신규 펫 획득: {PetName} (Level 1)", selectedTemplate.Name);
                }

                // 6-5. 천장 카운트 업데이트
                if (rarity == Rarity.Legendary)
                {
                    character.PetGachaCount = 0; // Legendary 획득 시 초기화
                    _logger.LogInformation("Legendary 획득: 천장 카운터 초기화 (0)");
                }
                else
                {
                    character.PetGachaCount += 1;
                }
            }

            // 7. Character 업데이트
            await _unitOfWork.Characters.UpdateAsync(character);

            // 8. 변경사항 저장 (Unit of Work 패턴)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("펫 가챠 완료: CharacterId={CharacterId}, Count={Count}, NewPets={NewCount}, Duplicates={DupCount}, NewPityCount={PityCount}",
                characterId, count, newPets.Count, duplicateRewards.Count, character.PetGachaCount);

            // 9. Response DTO 반환
            return new PetGachaResponseDto
            {
                Pets = newPets,
                DuplicateRewards = duplicateRewards,
                CurrentPityCount = character.PetGachaCount,
                RemainingCrystal = character.Crystal,
                TotalGoldFromDuplicates = totalGoldFromDuplicates
            };
        }

        /// <summary>
        /// 펫 ID로 펫 상세 정보를 조회합니다.
        /// </summary>
        public async Task<PetDto> GetPetByIdAsync(int petId, CancellationToken cancellationToken = default)
        {
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                _logger.LogWarning("펫 조회 실패: 펫을 찾을 수 없습니다. PetId={PetId}", petId);
                throw new InvalidOperationException($"펫을 찾을 수 없습니다. (ID: {petId})");
            }

            // Entity → DTO 변환
            return new PetDto
            {
                Id = pet.Id,
                TemplateName = pet.PetTemplate.Name,
                RarityName = pet.PetTemplate.Rarity.ToString(),
                Level = pet.Level,
                CurrentAttack = pet.CurrentAttack,
                CurrentMana = pet.CurrentMana,
                ImageUrl = $"/resources/pets/{pet.TemplateId}.png", // 클라이언트 리소스 경로
                IsEquipped = false // TODO: EquippedPets 조회 필요
            };
        }

        /// <summary>
        /// 특정 캐릭터가 소유한 모든 펫을 조회합니다.
        /// </summary>
        public async Task<List<PetDto>> GetPetsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            var pets = await _unitOfWork.Pets.GetByCharacterIdAsync(characterId, cancellationToken);

            // Entity → DTO 변환
            return pets.Select(pet => new PetDto
            {
                Id = pet.Id,
                TemplateName = pet.PetTemplate.Name,
                RarityName = pet.PetTemplate.Rarity.ToString(),
                Level = pet.Level,
                CurrentAttack = pet.CurrentAttack,
                CurrentMana = pet.CurrentMana,
                ImageUrl = $"/resources/pets/{pet.TemplateId}.png",
                IsEquipped = false // TODO: EquippedPets 조회 필요
            }).ToList();
        }

        /// <summary>
        /// 펫 레벨업을 수행합니다.
        /// 1. 펫 조회 및 소유권 검증
        /// 2. 레벨 상한 검증 (50)
        /// 3. 비용 계산 및 골드 차감
        /// 4. 스탯 계산 (BaseAttack + (Level-1) * 10)
        /// 5. 레벨 증가
        /// </summary>
        public async Task<PetLevelUpResponseDto> LevelUpPetAsync(int petId, Guid characterId, CancellationToken cancellationToken = default)
        {
            // 1. 펫 조회 (Include Template for BaseAttack/BaseMana)
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                _logger.LogWarning("펫 레벨업 실패: 펫을 찾을 수 없습니다. PetId={PetId}", petId);
                throw new InvalidOperationException($"펫을 찾을 수 없습니다. (ID: {petId})");
            }

            // 2. 소유권 검증
            if (pet.CharacterId != characterId)
            {
                _logger.LogWarning("펫 레벨업 실패: 소유권이 없습니다. PetId={PetId}, OwnerId={OwnerId}, RequesterId={RequesterId}",
                    petId, pet.CharacterId, characterId);
                throw new InvalidOperationException($"이 펫에 대한 권한이 없습니다. (PetId: {petId})");
            }

            // 3. 레벨 상한 검증
            if (pet.Level >= 50)
            {
                _logger.LogWarning("펫 레벨업 실패: 최대 레벨 도달. PetId={PetId}, Level={Level}", petId, pet.Level);
                throw new InvalidOperationException($"이미 최대 레벨입니다. (현재: {pet.Level})");
            }

            // 4. 캐릭터 조회 (골드 차감용)
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
            {
                _logger.LogWarning("펫 레벨업 실패: 캐릭터를 찾을 수 없습니다. CharacterId={CharacterId}", characterId);
                throw new KeyNotFoundException($"캐릭터를 찾을 수 없습니다. (ID: {characterId})");
            }

            // 5. 비용 계산: 100 * (1.5 ^ (Level - 1))
            int costGold = (int)(100 * Math.Pow(1.5, pet.Level - 1));

            // 6. 골드 검증
            if (character.Gold < costGold)
            {
                _logger.LogWarning("펫 레벨업 실패: 골드 부족. 필요={Cost}, 보유={Gold}", costGold, character.Gold);
                throw new InvalidOperationException($"골드가 부족합니다. (필요: {costGold}, 보유: {character.Gold})");
            }

            // 7. 골드 차감
            character.Gold -= costGold;

            // 8. 레벨 증가
            pet.Level += 1;

            // 9. 스탯 재계산
            pet.CurrentAttack = pet.PetTemplate.BaseAttack + (pet.Level - 1) * 10;
            pet.CurrentMana = pet.PetTemplate.BaseMana + (pet.Level - 1) * 5;
            pet.UpdatedAt = DateTime.UtcNow;

            // 10. 업데이트
            await _unitOfWork.Pets.UpdateAsync(pet, cancellationToken);
            await _unitOfWork.Characters.UpdateAsync(character);

            // 11. 변경사항 저장
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("펫 레벨업 완료: PetId={PetId}, NewLevel={Level}, NewAttack={Attack}, NewMana={Mana}, CostGold={Cost}",
                petId, pet.Level, pet.CurrentAttack, pet.CurrentMana, costGold);

            // 12. Response DTO 반환
            return new PetLevelUpResponseDto
            {
                PetId = pet.Id,
                NewLevel = pet.Level,
                NewAttack = pet.CurrentAttack,
                NewMana = pet.CurrentMana,
                CostGold = costGold,
                RemainingGold = character.Gold
            };
        }

        /// <summary>
        /// 펫을 슬롯에 장착합니다.
        /// 1. 펫 조회 및 소유권 검증
        /// 2. 슬롯 범위 검증 (1-3)
        /// 3. 같은 펫이 다른 슬롯에 있으면 해제
        /// 4. 해당 슬롯에 다른 펫이 있으면 해제
        /// 5. 새 펫 장착
        /// 6. 모든 장착 펫 조회 및 버프 계산
        /// </summary>
        public async Task<PetEquipResponseDto> EquipPetAsync(Guid characterId, int petId, int slotIndex, CancellationToken cancellationToken = default)
        {
            // 1. 슬롯 범위 검증
            if (slotIndex < 1 || slotIndex > 3)
            {
                _logger.LogWarning("펫 장착 실패: 잘못된 슬롯 인덱스. SlotIndex={SlotIndex}", slotIndex);
                throw new InvalidOperationException($"슬롯 인덱스는 1-3 범위여야 합니다. (입력: {slotIndex})");
            }

            // 2. 펫 조회 (Include Template for Buff 계산)
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                _logger.LogWarning("펫 장착 실패: 펫을 찾을 수 없습니다. PetId={PetId}", petId);
                throw new InvalidOperationException($"펫을 찾을 수 없습니다. (ID: {petId})");
            }

            // 3. 소유권 검증
            if (pet.CharacterId != characterId)
            {
                _logger.LogWarning("펫 장착 실패: 소유권이 없습니다. PetId={PetId}, OwnerId={OwnerId}, RequesterId={RequesterId}",
                    petId, pet.CharacterId, characterId);
                throw new InvalidOperationException($"이 펫에 대한 권한이 없습니다. (PetId: {petId})");
            }

            // 4. 같은 펫이 다른 슬롯에 장착되어 있는지 확인 (중복 장착 방지)
            var existingEquip = await _unitOfWork.EquippedPets.GetByPetIdAsync(petId, cancellationToken);
            if (existingEquip != null)
            {
                // 이미 다른 슬롯에 장착됨 → 해제
                _logger.LogInformation("펫 중복 장착 방지: 기존 슬롯 {OldSlot} 해제", existingEquip.SlotIndex);
                await _unitOfWork.EquippedPets.DeleteAsync(existingEquip, cancellationToken);
            }

            // 5. 해당 슬롯에 다른 펫이 있는지 확인 (슬롯 교체)
            var slotOccupant = await _unitOfWork.EquippedPets.GetBySlotAsync(characterId, slotIndex, cancellationToken);
            if (slotOccupant != null)
            {
                // 슬롯 점유 중 → 해제
                _logger.LogInformation("슬롯 교체: 슬롯 {SlotIndex}의 기존 펫 {OldPetId} 해제", slotIndex, slotOccupant.PetId);
                await _unitOfWork.EquippedPets.DeleteAsync(slotOccupant, cancellationToken);
            }

            // 6. 새 펫 장착
            var newEquip = new EquippedPets
            {
                CharacterId = characterId,
                SlotIndex = slotIndex,
                PetId = petId,
                EquippedAt = DateTime.UtcNow
            };

            await _unitOfWork.EquippedPets.AddAsync(newEquip, cancellationToken);

            // 7. 변경사항 저장
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("펫 장착 완료: PetId={PetId}, SlotIndex={SlotIndex}, CharacterId={CharacterId}",
                petId, slotIndex, characterId);

            // 8. 모든 장착 펫 조회 및 버프 계산
            var equippedPets = await GetEquippedPetsAsync(characterId, cancellationToken);

            // 9. 총 버프 계산
            int totalBuffAttack = equippedPets.Sum(ep => ep.BuffAttack);
            int totalBuffMana = equippedPets.Sum(ep => ep.BuffMana);

            return new PetEquipResponseDto
            {
                CharacterId = characterId,
                EquippedPets = equippedPets,
                TotalBuffAttack = totalBuffAttack,
                TotalBuffMana = totalBuffMana
            };
        }

        /// <summary>
        /// 특정 슬롯의 펫을 해제합니다.
        /// </summary>
        public async Task UnequipPetAsync(Guid characterId, int slotIndex, CancellationToken cancellationToken = default)
        {
            // 1. 슬롯 범위 검증
            if (slotIndex < 1 || slotIndex > 3)
            {
                _logger.LogWarning("펫 해제 실패: 잘못된 슬롯 인덱스. SlotIndex={SlotIndex}", slotIndex);
                throw new InvalidOperationException($"슬롯 인덱스는 1-3 범위여야 합니다. (입력: {slotIndex})");
            }

            // 2. 해당 슬롯에 장착된 펫 조회
            var equippedPet = await _unitOfWork.EquippedPets.GetBySlotAsync(characterId, slotIndex, cancellationToken);
            if (equippedPet == null)
            {
                _logger.LogWarning("펫 해제 실패: 슬롯에 장착된 펫이 없습니다. CharacterId={CharacterId}, SlotIndex={SlotIndex}",
                    characterId, slotIndex);
                throw new InvalidOperationException($"슬롯 {slotIndex}에 장착된 펫이 없습니다.");
            }

            // 3. 펫 해제
            await _unitOfWork.EquippedPets.DeleteAsync(equippedPet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("펫 해제 완료: CharacterId={CharacterId}, SlotIndex={SlotIndex}, PetId={PetId}",
                characterId, slotIndex, equippedPet.PetId);
        }

        /// <summary>
        /// 캐릭터가 장착한 모든 펫을 조회하고 버프를 계산합니다.
        /// </summary>
        public async Task<List<EquippedPetDto>> GetEquippedPetsAsync(Guid characterId, CancellationToken cancellationToken = default)
        {
            // 1. 장착된 펫 조회 (Include Pet, PetTemplate)
            var equippedPets = await _unitOfWork.EquippedPets.GetByCharacterIdAsync(characterId, cancellationToken);

            // 2. Entity → DTO 변환 및 버프 계산
            return equippedPets.Select(ep => new EquippedPetDto
            {
                SlotIndex = ep.SlotIndex,
                PetId = ep.PetId,
                PetName = ep.Pet.PetTemplate.Name,
                Level = ep.Pet.Level,
                BuffAttack = (int)(ep.Pet.CurrentAttack * 0.1), // 10% 버프
                BuffMana = (int)(ep.Pet.CurrentMana * 0.1)
            }).ToList();
        }

        /// <summary>
        /// 펫을 삭제합니다. (장착 중인 펫은 삭제 불가)
        /// </summary>
        public async Task DeletePetAsync(int petId, Guid characterId, CancellationToken cancellationToken = default)
        {
            // 1. 펫 조회
            var pet = await _unitOfWork.Pets.GetByIdAsync(petId, cancellationToken);
            if (pet == null)
            {
                _logger.LogWarning("펫 삭제 실패: 펫을 찾을 수 없습니다. PetId={PetId}", petId);
                throw new InvalidOperationException($"펫을 찾을 수 없습니다. (ID: {petId})");
            }

            // 2. 소유권 검증
            if (pet.CharacterId != characterId)
            {
                _logger.LogWarning("펫 삭제 실패: 소유권이 없습니다. PetId={PetId}, OwnerId={OwnerId}, RequesterId={RequesterId}",
                    petId, pet.CharacterId, characterId);
                throw new InvalidOperationException($"이 펫에 대한 권한이 없습니다. (PetId: {petId})");
            }

            // 3. 장착 상태 확인 (장착 중이면 삭제 불가)
            var equippedPet = await _unitOfWork.EquippedPets.GetByPetIdAsync(petId, cancellationToken);
            if (equippedPet != null)
            {
                _logger.LogWarning("펫 삭제 실패: 장착 중인 펫입니다. PetId={PetId}, SlotIndex={SlotIndex}",
                    petId, equippedPet.SlotIndex);
                throw new InvalidOperationException($"장착 중인 펫은 삭제할 수 없습니다. (슬롯: {equippedPet.SlotIndex})");
            }

            // 4. 펫 삭제
            await _unitOfWork.Pets.DeleteAsync(pet, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("펫 삭제 완료: PetId={PetId}, CharacterId={CharacterId}", petId, characterId);
        }
    }
}
