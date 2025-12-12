using IdleRPG.Application.BattleLog.Services;
using IdleRPG.Application.DTOs.Battle;
using IdleRPG.Application.Interfaces;
using IdleRPG.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IdleRPG.Infrastructure.Service
{
    /// <summary>
    /// 전투 로그 관리 서비스 구현
    ///
    /// 책임:
    /// - 전투 로그 생성 및 Repository 추가 (SaveChanges는 호출자가 처리)
    /// - 전투 로그 조회 (페이징, 최근 N개)
    /// - 전투 통계 계산
    ///
    /// 특징:
    /// - 모든 전투(Stage, SpecialDungeon, PVP)에서 동일한 로그 생성 로직 사용
    /// - 횡단 관심사(Cross-Cutting Concern) 중앙 관리
    /// - 트랜잭션 경계는 호출자(StageService 등)가 제어
    /// </summary>
    public class BattleLogService : IBattleLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BattleLogService> _logger;

        public BattleLogService(
            IUnitOfWork unitOfWork,
            ILogger<BattleLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Domain.Entities.BattleLog> CreateAndSaveLogAsync(
            Guid characterId,
            Guid monsterId,
            CombatResultDto combatResult,
            int experienceGained,
            int goldGained,
            int? dungeonStageId = null)
        {
            var log = new Domain.Entities.BattleLog
            {
                CharacterId = characterId,
                MonsterId = monsterId,
                DungeonStageId = dungeonStageId,
                IsVictory = combatResult.IsVictory,
                ExperienceGained = experienceGained,
                GoldGained = goldGained,
                DamageDealt = combatResult.Statistics.TotalDamageDealt,
                DamageTaken = combatResult.Statistics.TotalDamageTaken,
                BattleDate = DateTime.UtcNow
            };

            await _unitOfWork.BattleLogs.AddAsync(log);

            _logger.LogInformation(
                "전투 로그 생성 - 캐릭터: {CharacterId}, 몬스터: {MonsterId}, 승리: {IsVictory}, 경험치: {Exp}, 골드: {Gold}",
                characterId, monsterId, combatResult.IsVictory, experienceGained, goldGained);

            // ⚠️ SaveChanges는 호출하지 않음 - 호출자가 트랜잭션으로 묶음
            return log;
        }

        public async Task<BattleLogsResponse> GetLogsAsync(Guid characterId, int page, int pageSize)
        {
            var logs = await _unitOfWork.BattleLogs.GetByCharacterIdAsync(characterId, page, pageSize);

            // Entity → DTO 변환
            var logDtos = logs.Select(log => new BattleLogDto
            {
                Id = log.Id,
                MonsterName = log.Monster.Name,
                MonsterLevel = log.Monster.Level,
                IsVictory = log.IsVictory,
                ExperienceGained = log.ExperienceGained,
                GoldGained = log.GoldGained,
                DamageDealt = log.DamageDealt,
                DamageTaken = log.DamageTaken,
                BattleDate = log.BattleDate
            }).ToList();

            // 전체 로그 수 계산 (페이징 정보용)
            // TODO: Repository에 카운트 메서드 추가 고려 (성능 개선)
            var allLogs = await _unitOfWork.BattleLogs.FindAsync(bl => bl.CharacterId == characterId);
            int totalCount = allLogs.Count();

            return new BattleLogsResponse
            {
                Logs = logDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<List<BattleLogDto>> GetRecentLogsAsync(Guid characterId, int count)
        {
            var logs = await _unitOfWork.BattleLogs.GetRecentByCharacterIdAsync(characterId, count);

            // Entity → DTO 변환
            return logs.Select(log => new BattleLogDto
            {
                Id = log.Id,
                MonsterName = log.Monster.Name,
                MonsterLevel = log.Monster.Level,
                IsVictory = log.IsVictory,
                ExperienceGained = log.ExperienceGained,
                GoldGained = log.GoldGained,
                DamageDealt = log.DamageDealt,
                DamageTaken = log.DamageTaken,
                BattleDate = log.BattleDate
            }).ToList();
        }

        public async Task<BattleStatistics> GetStatsAsync(Guid characterId)
        {
            return await _unitOfWork.BattleLogs.GetStatsByCharacterIdAsync(characterId);
        }
    }
}
