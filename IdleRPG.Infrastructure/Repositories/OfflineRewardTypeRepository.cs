using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories;

/// <summary>
/// 오프라인 보상 타입 Repository 구현
/// </summary>
public class OfflineRewardTypeRepository : IOfflineRewardTypeRepository
{
    private readonly GameDBContext _context;

    public OfflineRewardTypeRepository(GameDBContext context)
    {
        _context = context;
    }

    public async Task<OfflineRewardType> GetDefaultAsync()
    {
        // 현재는 시딩된 1개의 보상 타입만 존재하므로 First 사용
        return await _context.OfflineRewardTypes.FirstAsync();
    }

    public async Task<OfflineRewardType?> GetByIdAsync(Guid id)
    {
        return await _context.OfflineRewardTypes.FindAsync(id);
    }
}
