namespace IdleRPG.Domain.ValueObjects
{
    /// <summary>
    /// 캐릭터 전투 스탯 Value Object (자동 성장 방식)
    /// - 기본 스탯 제거: 레벨업 시 전투 스탯 자동 증가
    /// - long 타입: 방치형 게임 특성상 숫자 급증 대비
    /// - 직업별 성장 공식: 나중에 JobType 추가 시 확장 가능
    /// </summary>
    public class CharacterStats
    {
        // 전투 스탯 (레벨업 시 자동 증가)
        public long Attack { get; }       // 공격력
        public long Defense { get; }      // 방어력
        public long MaxHealth { get; }    // 최대 체력
        public float CritRate { get; }    // 크리티컬 확률 (0.0 ~ 1.0)
        public float CritDamage { get; }  // 크리티컬 데미지 배율 (예: 1.5 = 150%)
        public float Evasion { get; }     // 회피율 (0.0 ~ 1.0)

        public CharacterStats(
            long attack, long defense, long maxHealth,
            float critRate, float critDamage, float evasion)
        {
            // 전투 스탯 검증
            if (attack < 0) throw new ArgumentException("Attack cannot be negative", nameof(attack));
            if (defense < 0) throw new ArgumentException("Defense cannot be negative", nameof(defense));
            if (maxHealth < 1) throw new ArgumentException("MaxHealth must be at least 1", nameof(maxHealth));
            if (critRate < 0 || critRate > 1) throw new ArgumentException("CritRate must be between 0 and 1", nameof(critRate));
            if (critDamage < 1) throw new ArgumentException("CritDamage must be at least 1", nameof(critDamage));
            if (evasion < 0 || evasion > 1) throw new ArgumentException("Evasion must be between 0 and 1", nameof(evasion));

            Attack = attack;
            Defense = defense;
            MaxHealth = maxHealth;
            CritRate = critRate;
            CritDamage = critDamage;
            Evasion = evasion;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CharacterStats)obj);
        }

        protected bool Equals(CharacterStats other)
        {
            return Attack == other.Attack &&
                   Defense == other.Defense &&
                   MaxHealth == other.MaxHealth &&
                   Math.Abs(CritRate - other.CritRate) < 0.0001f &&
                   Math.Abs(CritDamage - other.CritDamage) < 0.0001f &&
                   Math.Abs(Evasion - other.Evasion) < 0.0001f;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Attack);
            hash.Add(Defense);
            hash.Add(MaxHealth);
            hash.Add(CritRate);
            hash.Add(CritDamage);
            hash.Add(Evasion);
            return hash.ToHashCode();
        }
    }
}