namespace IdleRPG.Application.DTOs.Pet
{
    /// <summary>
    /// 펫 정보를 클라이언트에 전달하는 DTO
    /// 가챠 결과, 인벤토리 조회, 상세 조회 등에 사용
    /// </summary>
    public class PetDto
    {
        /// <summary>
        /// 펫 인스턴스 ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 펫 템플릿 이름 (예: "Fire Dragon")
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 희귀도 이름 (Common, Rare, Epic, Legendary)
        /// </summary>
        public string RarityName { get; set; } = string.Empty;

        /// <summary>
        /// 펫 레벨 (1-50)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 현재 공격력
        /// </summary>
        public int CurrentAttack { get; set; }

        /// <summary>
        /// 현재 마나
        /// </summary>
        public int CurrentMana { get; set; }

        /// <summary>
        /// 펫 이미지 URL (클라이언트 리소스 경로)
        /// 서버는 TemplateId 또는 TemplateName 기반으로 생성
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 장착 여부 (목록 조회 시 사용)
        /// </summary>
        public bool IsEquipped { get; set; }
    }
}
