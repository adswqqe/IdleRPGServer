/*
 * =====================================================================================
 *   Design Pattern: Master-Instance Pattern
 * =====================================================================================
 * 게임 아이템을 효율적으로 관리하기 위한 데이터 분리 패턴입니다.
 *
 * 구성 요소:
 *   - ItemTemplate (Master): 모든 플레이어가 공유하는 불변 데이터
 *     (이름, 설명, 아이콘, 최대 중첩 수량 등)
 *   - PlayerItem (Instance): 플레이어 개인의 가변 데이터
 *     (누가, 어떤 아이템을, 몇 개 소유하는지)
 *
 * 장점:
 *   - 메모리 효율성: 수천 명이 동일 아이템을 소유해도 마스터 데이터는 1회만 로드
 *   - 데이터 관리 용이성: ItemTemplate 수정 시 모든 인스턴스에 일괄 반영
 *   - 데이터 정규화: 데이터베이스 중복 최소화
 *
 * 예시:
 *   - ItemTemplate: "체력 물약" (이름, 설명, 아이콘 등 공유 데이터)
 *   - PlayerItem: 플레이어A가 "체력 물약" 50개 소유 (개인별 수량 데이터)
 * =====================================================================================
 */

using IdleRPG.Domain.Enums;
using System;

namespace IdleRPG.Domain.Entities
{
    /// <summary>
    /// 중첩 가능한 아이템(소모품, 재료 등)의 마스터 데이터입니다.
    /// Master-Instance 패턴의 'Master' 역할을 하며, 모든 플레이어가 공유하는 불변 정보를 담습니다.
    /// Equipment와 달리, 각 아이템은 고유한 상태(강화 레벨 등)를 가지지 않습니다.
    /// </summary>
    public class ItemTemplate
    {
        /// <summary>
        /// 아이템 템플릿의 고유 식별자
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 아이템 이름 (UI 표시용)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 아이템 설명 (UI 표시용)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 아이템 종류 (소모품, 재료, 퀘스트 등)
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// 아이템 아이콘 이미지 URL (선택 사항)
        /// </summary>
        public string? IconUrl { get; set; }

        /// <summary>
        /// 인벤토리 한 슬롯에 최대로 중첩 가능한 수량
        /// </summary>
        public int MaxStackSize { get; set; } = 999;
    }
}
