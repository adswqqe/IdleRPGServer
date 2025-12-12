using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// CharacterBattleProgress Repository 구현체입니다.
/// 전투 스테이지를 통한 캐릭터 진행 상황을 추적합니다.
/// </summary>
public class CharacterBattleProgressRepository : ICharacterBattleProgressRepository
{
    private readonly GameDBContext _context;

    public CharacterBattleProgressRepository(GameDBContext context)
    {
        _context = context;
    }

    // ICharacterBattleProgressRepository 전용 메서드

    /// <summary>
    /// 특정 캐릭터의 전투 진행 상황을 가져옵니다.
    /// 진행 상황 레코드가 없으면 새로 생성합니다.
    /// 효율적인 조회를 위해 고유 CharacterId 인덱스를 사용합니다.
    /// </summary>
    public async Task<CharacterBattleProgress> GetOrCreateByCharacterIdAsync(Guid characterId)
    {
        var progress = await _context.CharacterBattleProgresses
            .SingleOrDefaultAsync(p => p.CharacterId == characterId);

        if (progress == null)
        {
            progress = new CharacterBattleProgress
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
                HighestStageClearedNormal = 0,
                HighestStageClearedHard = 0,
                HighestStageClearedNightmare = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CharacterBattleProgresses.AddAsync(progress);
        }

        return progress;
    }

    // IRepository<CharacterBattleProgress> 인터페이스 구현

    public async Task<IEnumerable<CharacterBattleProgress>> GetAllAsync()
    {
        return await _context.CharacterBattleProgresses.ToListAsync();
    }

    public async Task<CharacterBattleProgress> AddAsync(CharacterBattleProgress entity)
    {
        await _context.CharacterBattleProgresses.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(CharacterBattleProgress entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.CharacterBattleProgresses.Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<CharacterBattleProgress>> FindAsync(Expression<Func<CharacterBattleProgress, bool>> predicate)
    {
        return await _context.CharacterBattleProgresses.Where(predicate).ToListAsync();
    }
}
