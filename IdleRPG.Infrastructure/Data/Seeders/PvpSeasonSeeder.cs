using IdleRPG.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Data.Seeders
{
    /// <summary>
    /// PVP 시즌 초기 데이터 생성기
    ///
    /// [시즌 설계]
    /// - Season 1: 2025-11-01 ~ 2026-02-01 (3개월, 활성 상태)
    /// - 활성 시즌은 최대 1개만 존재
    /// - Soft Reset 공식: (기존 레이팅 + 1000) / 2
    ///
    /// [시드 데이터 목적]
    /// - 애플리케이션 시작 시 기본 시즌 자동 생성
    /// - PVP 기능 즉시 사용 가능
    /// </summary>
    public class PvpSeasonSeeder
    {
        private readonly GameDBContext _context;
        private readonly ILogger<PvpSeasonSeeder> _logger;

        public PvpSeasonSeeder(GameDBContext context, ILogger<PvpSeasonSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// PVP 시즌 시드 데이터 생성 (Idempotent)
        /// </summary>
        public async Task SeedAsync()
        {
            // 이미 데이터가 있으면 스킵 (Idempotent 보장)
            if (await _context.Set<PvpSeason>().AnyAsync())
            {
                _logger.LogInformation("PVP 시즌 데이터가 이미 존재합니다. Seed 작업을 건너뜁니다.");
                return;
            }

            _logger.LogInformation("PVP 시즌 시드 데이터 생성 시작...");

            // Season 1 생성
            var season1 = new PvpSeason
            {
                SeasonNumber = 1,
                StartDate = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Set<PvpSeason>().AddAsync(season1);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "PVP 시즌 1 생성 완료 (기간: {StartDate:yyyy-MM-dd} ~ {EndDate:yyyy-MM-dd}, 활성: {IsActive})",
                season1.StartDate,
                season1.EndDate,
                season1.IsActive);
        }
    }
}
