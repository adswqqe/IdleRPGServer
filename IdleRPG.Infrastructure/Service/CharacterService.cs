using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Application.Interfaces;
using CharacterEntity = IdleRPG.Domain.Entities.Character;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace IdleRPG.Infrastructure.Service
{
    public class CharacterService : ICharacterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CharacterService> _logger;
        private readonly int _maxCharacterCount = 3;

        public CharacterService(IUnitOfWork unitOfWork, ILogger<CharacterService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        
        public async Task<CharacterDto> CreateCharacterAsync(Guid playerId, CreateCharacterDto dto)
        {
            int count = await _unitOfWork.Characters.CountByPlayerIdAsync(playerId);

            if (count >= _maxCharacterCount)
                throw new InvalidOperationException("최대 3개까지만 생성 가능합니다");

            var character = new CharacterEntity()
            {
                PlayerId = playerId,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Level = 1,
                Experience = 0,
                Stats = new CharacterStats(
                    attack: 10,
                    defense: 5,
                    maxHealth: 100,
                    critRate: 0.05f,
                    critDamage: 1.5f,
                    evasion: 0.05f,
                    attackSpeed: 1.0f  // 기본 공격 속도
                ),
            };

            await _unitOfWork.Characters.AddAsync(character);
            await _unitOfWork.SaveChangesAsync();
            return CreateCharacterDto(character);
        }

        public async Task<CharacterDto?> GetCharacterByIdAsync(Guid characterId)
        {
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);

            return character == null ? null : CreateCharacterDto(character);
        }
        
        public async Task<List<CharacterDto>> GetPlayerCharactersAsync(Guid playerId)
        {
            var characters = await _unitOfWork.Characters.GetByPlayerIdAsync(playerId);
            var dtoList = new List<CharacterDto>();

            foreach (var character in characters)
            {
                dtoList.Add(CreateCharacterDto(character));
            }

            
            return dtoList;
        }

        public async Task DeleteCharacterAsync(Guid characterId)
        {
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터 없음");

            _unitOfWork.Characters.Delete(character);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CharacterDto> AddExperienceAsync(Guid characterId, int amount)
        {
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 레벨업 처리 (SaveChanges 없이)
            ProcessExperienceGain(character, amount);

            character.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return CreateCharacterDto(character);
        }

        public async Task<(bool IsLevelUp, int NewLevel, int LevelUps)> AddExperienceWithResultAsync(Guid characterId, int amount)
        {
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            // 레벨업 처리 (SaveChanges 없이)
            var (isLevelUp, levelUps) = ProcessExperienceGain(character, amount);

            character.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return (isLevelUp, character.Level, levelUps);
        }

        /// <summary>
        /// 경험치 추가 및 레벨업 처리 (SaveChanges 없음)
        /// OfflineRewardService 등에서 여러 작업을 한 트랜잭션으로 처리할 때 사용
        /// </summary>
        /// <returns>(IsLevelUp: 레벨업 여부, LevelUps: 레벨업 횟수)</returns>
        public (bool IsLevelUp, int LevelUps) ProcessExperienceGain(CharacterEntity character, int amount)
        {
            character.Experience += amount;

            int levelUps = 0;

            // 레벨업 체크 및 자동 성장
            while (character.Experience >= GetRequiredExp(character.Level))
            {
                character.Experience -= GetRequiredExp(character.Level);
                character.Level++;
                levelUps++;

                // 레벨업 시 전투 스탯 자동 증가
                // TODO: 직업별 성장 공식 추가 시 character.Job에 따라 분기
                character.Stats = new CharacterStats(
                    attack: character.Stats.Attack + 10,      // 공격력 +10
                    defense: character.Stats.Defense + 5,     // 방어력 +5
                    maxHealth: character.Stats.MaxHealth + 50, // 최대 체력 +50
                    critRate: character.Stats.CritRate,       // 크리티컬 확률 유지
                    critDamage: character.Stats.CritDamage,   // 크리티컬 배율 유지
                    evasion: character.Stats.Evasion,         // 회피율 유지
                    attackSpeed: character.Stats.AttackSpeed  // 공격 속도 유지 (장비로만 증가)
                );
            }

            character.UpdatedAt = DateTime.UtcNow;

            return (levelUps > 0, levelUps);
        }

        private int GetRequiredExp(int level)
        {
            return level * 100; // 레벨 * 100
        }

        private CharacterDto CreateCharacterDto(CharacterEntity character)
        {
            return new CharacterDto()
            {
                PlayerId = character.PlayerId,
                Id = character.Id,
                CreatedAt = character.CreatedAt,
                Level = character.Level,
                Experience = character.Experience,
                Gold = character.Gold,
                Crystal = character.Crystal,
                LastLoginTime = character.LastLoginTime,
                UpdatedAt = character.UpdatedAt,
                // 전투 스탯 (중첩 구조)
                Stats = new CharacterStatsDto
                {
                    Attack = character.Stats.Attack,
                    Defense = character.Stats.Defense,
                    MaxHealth = character.Stats.MaxHealth,
                    CritRate = character.Stats.CritRate,
                    CritDamage = character.Stats.CritDamage,
                    Evasion = character.Stats.Evasion,
                    AttackSpeed = character.Stats.AttackSpeed
                }
            };
        }
    }
}