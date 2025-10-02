using IdleRPG.Application.Character.Services;
using IdleRPG.Application.DTOs.Characters;
using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace IdleRPG.Infrastructure.Service
{
    public class CharacterService : ICharacterService
    {
        private readonly ICharacterRepository _repository;
        private readonly ILogger<CharacterService> _logger;
        private readonly int _maxCharacterCount = 3;
        
        public CharacterService(ICharacterRepository repository, ILogger<CharacterService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        public async Task<CharacterDto> CreateCharacterAsync(Guid playerId, CreateCharacterDto dto)
        {
            int count = await _repository.CountByPlayerIdAsync(playerId);

            if (count >= _maxCharacterCount)
                throw new InvalidOperationException("최대 3개까지만 생성 가능합니다");

            var character = new Character()
            {
                PlayerId = playerId,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Level = 1,
                Experience = 0,
                Stats = new CharacterStats(5, 5, 5, 10),
            };

            await _repository.AddAsync(character);
            await _repository.SaveChangesAsync();
            return CreateCharacterDto(character);
        }

        public async Task<CharacterDto?> GetCharacterByIdAsync(Guid characterId)
        {
            var character = await _repository.GetByIdAsync(characterId);

            return character == null ? null : CreateCharacterDto(character);
        }
        
        public async Task<List<CharacterDto>> GetPlayerCharactersAsync(Guid playerId)
        {
            var characters = await _repository.GetByPlayerIdAsync(playerId);
            var dtoList = new List<CharacterDto>();

            foreach (var character in characters)
            {
                dtoList.Add(CreateCharacterDto(character));
            }

            return dtoList;
        }

        public async Task DeleteCharacterAsync(Guid characterId)
        {
            var character = await _repository.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터 없음");
                
            _repository.Delete(character);
            await _repository.SaveChangesAsync();
        }

        public async Task<CharacterDto> AddExperienceAsync(Guid characterId, int amount)
        {
            var character = await _repository.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            character.Experience += amount;

            // 레벨업 체크
            while (character.Experience >= GetRequiredExp(character.Level))
            {
                character.Experience -= GetRequiredExp(character.Level);
                character.Level++;
                character.StatPoints += 5; // 레벨업 시 스탯 포인트 5개 지급
            }

            character.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
            
            return CreateCharacterDto(character);
        }

        public async Task<CharacterDto> AllocateStatPointsAsync(Guid characterId, int strength, int dexterity, int intelligence, int vitality)
        {
            var character = await _repository.GetByIdAsync(characterId);
            if (character == null)
                throw new InvalidOperationException("캐릭터를 찾을 수 없습니다");

            int totalPoints = strength + dexterity + intelligence + vitality;
            
            if (totalPoints > character.StatPoints)
                throw new InvalidOperationException("보유한 스탯 포인트가 부족합니다");

            if (strength < 0 || dexterity < 0 || intelligence < 0 || vitality < 0)
                throw new InvalidOperationException("스탯은 음수일 수 없습니다");

            // CharacterStats는 Value Object이므로 새로 생성
            character.Stats = new CharacterStats(
                character.Stats.Strength + strength,
                character.Stats.Dexterity + dexterity,
                character.Stats.Intelligence + intelligence,
                character.Stats.Vitality + vitality
            );

            character.StatPoints -= totalPoints;
            character.UpdatedAt = DateTime.UtcNow;
            
            await _repository.SaveChangesAsync();
            
            return CreateCharacterDto(character);
        }

        private int GetRequiredExp(int level)
        {
            return level * 100; // 레벨 * 100
        }

        private CharacterDto CreateCharacterDto(Character character)
        {
            return new CharacterDto()
            {
                PlayerId = character.PlayerId,
                Id = character.Id,
                CreatedAt = character.CreatedAt,
                Dexterity = character.Stats.Dexterity,
                Intelligence = character.Stats.Intelligence,
                Strength = character.Stats.Strength,
                Vitality = character.Stats.Vitality,
                Level = character.Level,
                Experience = character.Experience,
                StatPoints = character.StatPoints,
                UpdatedAt = character.UpdatedAt,
            };   
        }
    }
}