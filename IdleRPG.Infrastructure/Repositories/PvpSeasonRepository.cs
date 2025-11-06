using IdleRPG.Domain.Entities;
using IdleRPG.Domain.Repositories;
using IdleRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdleRPG.Infrastructure.Repositories
{
    /// <summary>
    /// PvpSeason Repository 구현 (EF Core)
    /// </summary>
    public class PvpSeasonRepository : IPvpSeasonRepository
    {
        private readonly GameDBContext _context;

        public PvpSeasonRepository(GameDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ID로 시즌 조회
        /// </summary>
        public async Task<PvpSeason?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.PvpSeasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        /// <summary>
        /// 현재 활성 시즌 조회 (IsActive = true)
        /// 시스템 전체에서 활성 시즌은 최대 1개만 존재
        /// </summary>
        public async Task<PvpSeason?> GetActiveSeasonAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PvpSeasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);
        }

        /// <summary>
        /// 시즌 번호로 조회
        /// </summary>
        public async Task<PvpSeason?> GetBySeasonNumberAsync(int seasonNumber, CancellationToken cancellationToken = default)
        {
            return await _context.PvpSeasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SeasonNumber == seasonNumber, cancellationToken);
        }

        /// <summary>
        /// 새 시즌 추가
        /// </summary>
        public async Task AddAsync(PvpSeason season, CancellationToken cancellationToken = default)
        {
            await _context.PvpSeasons.AddAsync(season, cancellationToken);
        }

        /// <summary>
        /// 시즌 정보 업데이트 (IsActive 토글, 시즌 종료 등)
        /// </summary>
        public async Task UpdateAsync(PvpSeason season, CancellationToken cancellationToken = default)
        {
            _context.PvpSeasons.Update(season);
            await Task.CompletedTask;
        }
    }
}
