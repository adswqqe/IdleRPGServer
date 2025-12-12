using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// PvpMatch Repository 구현 (EF Core)
    /// PVP 매치 기록 저장 및 조회 (Attacker/Defender 양방향)
    /// </summary>
    public class PvpMatchRepository : IPvpMatchRepository
    {
        private readonly GameDBContext _context;

        public PvpMatchRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ID로 PVP 매치 조회
        /// </summary>
        public async Task<PvpMatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PvpMatches
                .AsNoTracking()
                .Include(pm => pm.Attacker)
                .Include(pm => pm.Defender)
                .Include(pm => pm.Winner)
                .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
        }

        /// <summary>
        /// 캐릭터별 매치 히스토리 조회 (페이징)
        /// Attacker 또는 Defender로 참여한 모든 매치 조회
        /// </summary>
        public async Task<(List<PvpMatch> matches, int totalCount)> GetMatchHistoryAsync(
            Guid characterId,
            int? seasonId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // 기본 쿼리: Attacker 또는 Defender로 참여한 매치
            var query = _context.PvpMatches
                .AsNoTracking()
                .Where(pm => pm.AttackerId == characterId || pm.DefenderId == characterId);

            // 시즌 필터링 (seasonId가 null이면 전체 시즌)
            if (seasonId.HasValue)
            {
                query = query.Where(pm => pm.SeasonId == seasonId.Value);
            }

            // 전체 개수 조회 (페이징 메타정보)
            int totalCount = await query.CountAsync(cancellationToken);

            // 페이징 + 정렬 (최신순)
            int skip = (page - 1) * pageSize;
            var matches = await query
                .OrderByDescending(pm => pm.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(pm => pm.Attacker)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .Include(pm => pm.Defender)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .Include(pm => pm.Season) // Season.SeasonNumber 조회
                .ToListAsync(cancellationToken);

            return (matches, totalCount);
        }

        /// <summary>
        /// 새 PVP 매치 추가
        /// </summary>
        public async Task AddAsync(PvpMatch match, CancellationToken cancellationToken = default)
        {
            await _context.PvpMatches.AddAsync(match, cancellationToken);
        }
    }
}
