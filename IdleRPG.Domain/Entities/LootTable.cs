/*
 * =====================================================================================
 *   Design Pattern: Loot Table Pattern
 * =====================================================================================
 * 게임 내 보상(아이템, 재화, 경험치)을 데이터 기반으로 관리하는 업계 표준 패턴입니다.
 *
 * 구성 요소:
 *   - LootTable: 재사용 가능한 보상 패키지 (예: "고블린 드랍", "보물상자 보상")
 *   - LootItem: 개별 보상 항목 (보상 종류, 확률 가중치, 수량 등)
 *
 * 작동 방식:
 *   1. IsGuaranteed=true 아이템: 100% 확정 지급
 *   2. IsGuaranteed=false 아이템: Weight 기반 확률 추첨 (NumberOfRolls 횟수만큼)
 *
 * 장점:
 *   - 유연성: 코드 변경 없이 데이터 수정만으로 보상 밸런싱
 *   - 재사용성: 하나의 LootTable을 던전, 퀘스트, PVP 등에서 공유
 *   - 확장성: 새로운 RewardType 추가로 시스템 확장 용이
 * =====================================================================================
 */

using System.Collections.Generic;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 재사용 가능한 보상 그룹(드랍 테이블)을 정의하는 마스터 데이터입니다.
    /// 하나의 LootTable은 여러 개의 LootItem을 포함할 수 있습니다.
    /// </summary>
    public class LootTable
    {
        /// <summary>
        /// LootTable의 고유 식별자
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// LootTable을 식별하기 위한 이름 (예: "고블린 숲 일반 드랍", "일일 접속 보상 1일차")
        /// 기획자 및 개발자 편의용입니다.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 이 테이블에서 확률 기반 아이템을 몇 번 추첨할지 결정합니다.
        /// 보장 보상(IsGuaranteed)은 1회만 지급되며, 이 횟수에 영향을 받지 않습니다.
        /// </summary>
        public int NumberOfRolls { get; set; } = 1;

        // Navigation Property
        /// <summary>
        /// 이 LootTable에 포함된 개별 보상 아이템 목록
        /// </summary>
        public virtual ICollection<LootItem> Items { get; set; } = new List<LootItem>();
    }
}
