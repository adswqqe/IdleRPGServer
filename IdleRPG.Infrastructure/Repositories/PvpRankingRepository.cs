using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Enums;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// PvpRanking Repository 구현 (EF Core)
    /// 복합키: (SeasonId, CharacterId)
    /// </summary>
    public class PvpRankingRepository : IPvpRankingRepository
    {
        private readonly GameDBContext _context;

        public PvpRankingRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 복합키로 랭킹 조회 (SeasonId, CharacterId)
        /// </summary>
        public async Task<PvpRanking?> GetByIdAsync(int seasonId, Guid characterId, CancellationToken cancellationToken = default)
        {
            // FindAsync는 Include 불가 → FirstOrDefaultAsync 사용
            return await _context.PvpRankings
                .AsNoTracking()
                .Include(pr => pr.Character)
                    .ThenInclude(c => c.Player) // Player.UserName 조회
                .FirstOrDefaultAsync(pr => pr.SeasonId == seasonId && pr.CharacterId == characterId, cancellationToken);
        }

        /// <summary>
        /// 여러 캐릭터의 랭킹 벌크 조회 (N+1 쿼리 방지)
        /// Redis에서 가져온 CharacterId 목록으로 DB 조회 시 사용
        /// </summary>
        public async Task<List<PvpRanking>> GetByCharacterIdsAsync(int seasonId, IEnumerable<Guid> characterIds, CancellationToken cancellationToken = default)
        {
            return await _context.PvpRankings
                .AsNoTracking()
                .Where(pr => pr.SeasonId == seasonId && characterIds.Contains(pr.CharacterId))
                .Include(pr => pr.Character)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Top N 랭킹 조회 (레이팅 내림차순)
        /// Redis 캐싱 미스 시 PostgreSQL Fallback 용도
        /// </summary>
        public async Task<List<PvpRanking>> GetTopRankingsAsync(int seasonId, int count, CancellationToken cancellationToken = default)
        {
            return await _context.PvpRankings
                .AsNoTracking()
                .Where(pr => pr.SeasonId == seasonId)
                .OrderByDescending(pr => pr.Rating)
                .Take(count)
                .Include(pr => pr.Character)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 특정 레이팅 주변 랭킹 조회 (내 주변 ±range등)
        /// </summary>
        public async Task<List<PvpRanking>> GetRankingsAroundAsync(int seasonId, int rating, int range, CancellationToken cancellationToken = default)
        {
            // 레이팅 범위 계산
            int minRating = Math.Max(0, rating - range * 10); // range=10 → ±100 레이팅
            int maxRating = rating + range * 10;

            return await _context.PvpRankings
                .AsNoTracking()
                .Where(pr => pr.SeasonId == seasonId && pr.Rating >= minRating && pr.Rating <= maxRating)
                .OrderByDescending(pr => pr.Rating)
                .Take(range * 2) // ±range등 조회
                .Include(pr => pr.Character)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 티어별 랭킹 조회 (페이징)
        /// </summary>
        public async Task<List<PvpRanking>> GetByTierAsync(int seasonId, PvpTier tier, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            int skip = (page - 1) * pageSize;

            return await _context.PvpRankings
                .AsNoTracking()
                .Where(pr => pr.SeasonId == seasonId && pr.Tier == tier)
                .OrderByDescending(pr => pr.Rating)
                .Skip(skip)
                .Take(pageSize)
                .Include(pr => pr.Character)
                    .ThenInclude(c => c.Player) // N+1 방지 (Player.UserName 조회)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 새 랭킹 추가 (첫 매칭 시)
        /// </summary>
        public async Task AddAsync(PvpRanking ranking, CancellationToken cancellationToken = default)
        {
            await _context.PvpRankings.AddAsync(ranking, cancellationToken);
        }

        /// <summary>
        /// 랭킹 정보 업데이트 (레이팅, 승패 횟수, 연승 등)
        /// </summary>
        public async Task UpdateAsync(PvpRanking ranking, CancellationToken cancellationToken = default)
        {
            _context.PvpRankings.Update(ranking);
            await Task.CompletedTask;
        }
    }
}
