using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// DungeonStage Repository 구현체입니다.
/// 던전 스테이지 정보에 대한 데이터 액세스 메서드를 제공합니다.
/// </summary>
public class DungeonStageRepository : IDungeonStageRepository
{
    private readonly GameDBContext _context;

    public DungeonStageRepository(GameDBContext context)
    {
        _context = context;
    }

    // IDungeonStageRepository 전용 메서드

    /// <summary>
    /// 지정된 레벨의 캐릭터가 접근 가능한 모든 던전 스테이지를 가져옵니다.
    /// 효율적인 필터링을 위해 RequiredLevel 인덱스를 사용합니다.
    /// </summary>
    public async Task<List<DungeonStage>> GetAccessibleStagesAsync(int characterLevel)
    {
        return await _context.DungeonStages
            .Where(d => d.RequiredLevel <= characterLevel)
            .OrderBy(d => d.Id)
            .ToListAsync();
    }

    /// <summary>
    /// ID로 던전 스테이지를 가져오며, Monster 탐색 속성을 포함합니다.
    /// </summary>
    public async Task<DungeonStage?> GetByIdWithMonsterAsync(int stageId)
    {
        return await _context.DungeonStages
            .Include(d => d.Monster)
            .SingleOrDefaultAsync(d => d.Id == stageId);
    }

    // IRepository<DungeonStage> 인터페이스 구현

    public async Task<IEnumerable<DungeonStage>> GetAllAsync()
    {
        return await _context.DungeonStages.ToListAsync();
    }

    public async Task<DungeonStage> AddAsync(DungeonStage entity)
    {
        await _context.DungeonStages.AddAsync(entity);
        return entity;
    }

    public async Task UpdateAsync(DungeonStage entity)
    {
        _context.DungeonStages.Update(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<DungeonStage>> FindAsync(Expression<Func<DungeonStage, bool>> predicate)
    {
        return await _context.DungeonStages.Where(predicate).ToListAsync();
    }
}
