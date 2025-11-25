using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// 퀘스트 Repository 구현
/// Infrastructure 계층에서 실제 DB 접근을 담당합니다.
/// </summary>
public class QuestRepository : IQuestRepository
{
    private readonly GameDBContext _context;

    public QuestRepository(GameDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ID로 퀘스트 조회
    /// </summary>
    public Task<Quest?> GetByIdAsync(Guid id)
    {
        return _context.Quests.SingleOrDefaultAsync(q => q.Id == id);
    }

    /// <summary>
    /// 캐릭터의 진행 중인 퀘스트 목록 조회
    /// </summary>
    public Task<List<Quest>> GetActiveQuestsByCharacterIdAsync(Guid characterId)
    {
        return _context.Quests
            .Where(q => q.CharacterId == characterId && q.Status == QuestStatus.InProgress)
            .ToListAsync();
    }

    /// <summary>
    /// 캐릭터가 특정 퀘스트를 이미 진행 중인지 확인
    /// </summary>
    public Task<bool> IsQuestInProgressAsync(Guid characterId, int questTemplateId)
    {
        return _context.Quests.AnyAsync(q =>
            q.CharacterId == characterId &&
            q.QuestTemplateId == questTemplateId &&
            q.Status == QuestStatus.InProgress);
    }

    /// <summary>
    /// 퀘스트 추가
    /// </summary>
    public async Task<Quest> AddAsync(Quest entity)
    {
        await _context.Quests.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// 모든 퀘스트 조회
    /// </summary>
    public async Task<IEnumerable<Quest>> GetAllAsync()
    {
        return await _context.Quests.ToListAsync();
    }

    /// <summary>
    /// 퀘스트 업데이트
    /// </summary>
    public Task UpdateAsync(Quest entity)
    {
        _context.Quests.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 조건에 맞는 퀘스트 검색
    /// </summary>
    public async Task<IEnumerable<Quest>> FindAsync(Expression<Func<Quest, bool>> predicate)
    {
        return await _context.Quests.Where(predicate).ToListAsync();
    }
}
