using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// CharacterDungeonProgress Repository 구현체입니다.
/// 던전 스테이지를 통한 캐릭터 진행 상황을 추적합니다.
/// </summary>
public class CharacterDungeonProgressRepository : ICharacterDungeonProgressRepository
{
    private readonly GameDBContext _context;

    public CharacterDungeonProgressRepository(GameDBContext context)
    {
        _context = context;
    }

    // ICharacterDungeonProgressRepository 전용 메서드

    /// <summary>
    /// 특정 캐릭터의 던전 진행 상황을 가져옵니다.
    /// 진행 상황 레코드가 없으면 새로 생성합니다.
    /// 효율적인 조회를 위해 고유 CharacterId 인덱스를 사용합니다.
    /// </summary>
    public async Task<CharacterDungeonProgress> GetOrCreateByCharacterIdAsync(Guid characterId)
    {
        var progress = await _context.CharacterDungeonProgresses
            .SingleOrDefaultAsync(p => p.CharacterId == characterId);

        if (progress == null)
        {
            progress = new CharacterDungeonProgress
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                HighestStageClearedNormal = 0,
                HighestStageClearedHard = 0,
                HighestStageClearedNightmare = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CharacterDungeonProgresses.AddAsync(progress);
        }

        return progress;
    }

    // IRepository<CharacterDungeonProgress> 인터페이스 구현

    public async Task<IEnumerable<CharacterDungeonProgress>> GetAllAsync()
    {
        return await _context.CharacterDungeonProgresses.ToListAsync();
    }

    public async Task<CharacterDungeonProgress> AddAsync(CharacterDungeonProgress entity)
    {
        await _context.CharacterDungeonProgresses.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(CharacterDungeonProgress entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.CharacterDungeonProgresses.Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<CharacterDungeonProgress>> FindAsync(Expression<Func<CharacterDungeonProgress, bool>> predicate)
    {
        return await _context.CharacterDungeonProgresses.Where(predicate).ToListAsync();
    }
}
