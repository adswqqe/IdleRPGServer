using IdleRPG.Application.DTOs.Gacha;
using IdleRPG.Application.Interfaces;
using IdleRPG.Application.Services;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Services
{
    /// <summary>
    /// 스킬 관련 비즈니스 로직 구현체
    /// </summary>
    public class SkillService : ISkillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GachaLogicService _gachaLogicService;
        private readonly ILogger<SkillService> _logger;

        public SkillService(
            IUnitOfWork unitOfWork,
            GachaLogicService gachaLogicService,
            ILogger<SkillService> logger)
        {
            _unitOfWork = unitOfWork;
            _gachaLogicService = gachaLogicService;
            _logger = logger;
        }

        /// <summary>
        /// 스킬 가챠를 수행합니다.
        /// 1. 캐릭터 조회 및 Crystal 검증
        /// 2. 현재 천장 카운트 확인
        /// 3. GachaLogicService로 희귀도 결정
        /// 4. 해당 희귀도의 스킬 중 랜덤 선택
        /// 5. CharacterSkill 생성 및 저장
        /// 6. Crystal 차감, 천장 카운트 업데이트
        /// </summary>
        public async Task<SkillDto> PerformGachaAsync(Guid characterId, int gachaCost = 100)
        {
            // 1. 캐릭터 조회
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
            {
                _logger.LogWarning("스킬 가챠 실패: 캐릭터를 찾을 수 없습니다. CharacterId={CharacterId}", characterId);
                throw new InvalidOperationException($"캐릭터를 찾을 수 없습니다. (ID: {characterId})");
            }

            // 2. Crystal 검증
            if (character.Crystal < gachaCost)
            {
                _logger.LogWarning("스킬 가챠 실패: Crystal 부족. 필요={Cost}, 보유={Crystal}", gachaCost, character.Crystal);
                throw new InvalidOperationException($"Crystal이 부족합니다. (필요: {gachaCost}, 보유: {character.Crystal})");
            }

            // 3. 희귀도 결정 (GachaLogicService 사용)
            var rarity = _gachaLogicService.DetermineRarity(character.GachaPityCount);
            _logger.LogInformation("스킬 가챠 희귀도 결정: {Rarity}, PityCount={PityCount}", rarity, character.GachaPityCount);

            // 4. 모든 스킬 템플릿 조회
            var allSkills = await _unitOfWork.SkillTemplates.GetAllAsync();

            // 5. 해당 희귀도의 스킬 중 랜덤 선택
            var selectedSkill = _gachaLogicService.SelectRandomSkill(rarity, allSkills);
            _logger.LogInformation("스킬 가챠 결과: {SkillName} ({Rarity})", selectedSkill.Name, selectedSkill.Rarity);

            // 6. CharacterSkill 생성
            var characterSkill = new CharacterSkill
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                SkillTemplateId = selectedSkill.Id,
                IsEquipped = false, // 미장착 상태
                AcquiredAt = DateTime.UtcNow
            };

            await _unitOfWork.CharacterSkills.AddAsync(characterSkill);

            // 7. Crystal 차감
            character.Crystal -= gachaCost;

            // 8. 천장 카운트 업데이트
            character.GachaPityCount = _gachaLogicService.GetPityCountAfterDraw(selectedSkill.Rarity, character.GachaPityCount);

            await _unitOfWork.Characters.UpdateAsync(character);

            // 9. 변경사항 저장 (Unit of Work 패턴)
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("스킬 가챠 완료: CharacterId={CharacterId}, SkillId={SkillId}, NewPityCount={PityCount}",
                characterId, selectedSkill.Id, character.GachaPityCount);

            // 10. DTO 반환
            return new SkillDto
            {
                Id = selectedSkill.Id,
                Name = selectedSkill.Name,
                Description = selectedSkill.Description,
                Rarity = selectedSkill.Rarity,
                Type = selectedSkill.Type
            };
        }
    }
}
