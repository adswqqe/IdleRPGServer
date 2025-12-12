using IdleRPG.Application.DTOs.Equipment;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EquipmentService> _logger;
        private const int MaxEnhancementLevel = 10;

        public EquipmentService(IUnitOfWork unitOfWork, ILogger<EquipmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            // 소유자 캐릭터 존재 확인
            var owner = await _unitOfWork.Characters.GetByIdAsync(dto.OwnerId);
            if (owner == null)
                throw new InvalidOperationException("소유자 캐릭터를 찾을 수 없습니다");

            var equipment = new Equipment
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Slot = dto.Slot,
                Rarity = dto.Rarity,
                OwnerId = dto.OwnerId,
                CharacterId = null, // 생성 시 미장착 상태
                EnhancementLevel = 0,
                BaseAttack = dto.BaseAttack,
                BaseDefense = dto.BaseDefense,
                BaseHp = dto.BaseHp,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Equipments.AddAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("장비 생성: {EquipmentId}, 소유자: {OwnerId}", equipment.Id, dto.OwnerId);

            return MapToDto(equipment);
        }

        public async Task<List<EquipmentDto>> GetEquippedItemsAsync(Guid characterId)
        {
            var equipments = await _unitOfWork.Equipments.GetEquippedByCharacterIdAsync(characterId);
            return equipments.Select(MapToDto).ToList();
        }

        public async Task<List<EquipmentDto>> GetInventoryAsync(Guid ownerId)
        {
            var equipments = await _unitOfWork.Equipments.GetInventoryByOwnerIdAsync(ownerId);
            return equipments.Select(MapToDto).ToList();
        }

        public async Task<EquipmentDto> EquipItemAsync(EquipItemDto dto)
        {
            // 1. 장비 존재 확인
            var equipment = await _unitOfWork.Equipments.GetByIdAsync(dto.EquipmentId);
            if (equipment == null)
                throw new InvalidOperationException("장비를 찾을 수 없습니다");

            // 2. 소유자 확인 (equipment.OwnerId == dto.CharacterId)
            if (equipment.OwnerId != dto.CharacterId)
                throw new InvalidOperationException("다른 캐릭터의 장비는 장착할 수 없습니다");

            // 3. 같은 슬롯에 이미 장착된 장비가 있는지 확인
            var existingEquipment = await _unitOfWork.Equipments.GetEquippedBySlotAsync(
                dto.CharacterId, equipment.Slot);

            // 4. 있으면 기존 장비 해제 (CharacterId = null)
            if (existingEquipment != null)
            {
                existingEquipment.CharacterId = null;
                existingEquipment.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Equipments.UpdateAsync(existingEquipment);
                
                _logger.LogInformation("기존 장비 자동 해제: {EquipmentId}, 슬롯: {Slot}", 
                    existingEquipment.Id, existingEquipment.Slot);
            }

            // 5. 새 장비 장착 (CharacterId = dto.CharacterId)
            equipment.CharacterId = dto.CharacterId;
            equipment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Equipments.UpdateAsync(equipment);

            // 한 번의 트랜잭션으로 모든 변경사항 커밋
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("장비 장착 성공: {EquipmentId}, 캐릭터: {CharacterId}, 슬롯: {Slot}", 
                equipment.Id, dto.CharacterId, equipment.Slot);

            return MapToDto(equipment);
        }

        public async Task<EquipmentDto> UnequipItemAsync(UnequipItemDto dto)
        {
            var equipment = await _unitOfWork.Equipments.GetByIdAsync(dto.EquipmentId);
            if (equipment == null)
                throw new InvalidOperationException("장비를 찾을 수 없습니다");

            if (equipment.CharacterId == null)
                throw new InvalidOperationException("이미 장착 해제된 장비입니다");

            equipment.CharacterId = null;
            equipment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Equipments.UpdateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("장비 해제: {EquipmentId}", dto.EquipmentId);

            return MapToDto(equipment);
        }

        public async Task<EquipmentDto> EnhanceEquipmentAsync(EnhanceEquipmentDto dto)
        {
            var equipment = await _unitOfWork.Equipments.GetByIdAsync(dto.EquipmentId);
            if (equipment == null)
                throw new InvalidOperationException("장비를 찾을 수 없습니다");

            if (equipment.EnhancementLevel >= MaxEnhancementLevel)
                throw new InvalidOperationException($"최대 강화 레벨({MaxEnhancementLevel})에 도달했습니다");

            // TODO: 강화 비용 차감 (Gold 소모)
            // TODO: 강화 확률 적용 (Week 3-4에서 구현)

            equipment.EnhancementLevel++;
            equipment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Equipments.UpdateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("장비 강화: {EquipmentId}, 강화 레벨: {Level}", 
                dto.EquipmentId, equipment.EnhancementLevel);

            return MapToDto(equipment);
        }

        public async Task<bool> DeleteEquipmentAsync(Guid equipmentId)
        {
            var equipment = await _unitOfWork.Equipments.GetByIdAsync(equipmentId);
            if (equipment == null)
                return false;

            _unitOfWork.Equipments.Delete(equipment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("장비 삭제: {EquipmentId}", equipmentId);

            return true;
        }

        private EquipmentDto MapToDto(Equipment equipment)
        {
            return new EquipmentDto
            {
                Id = equipment.Id,
                Name = equipment.Name,
                Slot = equipment.Slot,
                Rarity = equipment.Rarity,
                OwnerId = equipment.OwnerId,
                CharacterId = equipment.CharacterId,
                EnhancementLevel = equipment.EnhancementLevel,
                BaseAttack = equipment.BaseAttack,
                BaseDefense = equipment.BaseDefense,
                BaseHp = equipment.BaseHp,
                TotalAttack = equipment.GetTotalAttack(),
                TotalDefense = equipment.GetTotalDefense(),
                TotalHp = equipment.GetTotalHp(),
                CreatedAt = equipment.CreatedAt,
                UpdatedAt = equipment.UpdatedAt
            };
        }
    }
}
