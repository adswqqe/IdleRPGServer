using IdleRPG.Domain.Enums;

namespace IdleRPG.Application.DTOs.Dungeon
{
    /// <summary>
    /// 던전 스테이지 클리어 요청 DTO
    /// 클라이언트가 서버에 "이 스테이지를 클리어했어요!"라고 알릴 때 사용
    ///
    /// [서버 중심 설계]
    /// - 클라이언트는 CharacterId + StageId + Difficulty 전송
    /// - 서버가 소유권 검증 (내 캐릭터인지 확인)
    /// - 서버가 모든 비즈니스 검증 수행 (레벨, 진행도, 치트 방지)
    /// - 보상 계산도 100% 서버에서 처리
    /// </summary>
    public class DungeonClearRequestDto
    {
        /// <summary>
        /// 도전하는 캐릭터의 ID
        /// 서버에서 소유권 검증: 이 캐릭터가 요청한 플레이어의 것인지 확인
        /// </summary>
        public Guid CharacterId { get; set; }

        /// <summary>
        /// 클리어한 스테이지 ID
        /// </summary>
        public int StageId { get; set; }

        /// <summary>
        /// 클리어한 난이도
        /// </summary>
        public DungeonDifficulty Difficulty { get; set; }
    }
}
