using System.ComponentModel.DataAnnotations;
namespace IdleRPG.Domain.Entities
{
    public class Character
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PlayerId { get; set; }
        public Player Player { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; }

        [Required, MaxLength(20)]
        public string CharacterClass { get; set; }

        public int Level { get; set; } = 1;
        public long Experience { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public bool IsMain { get; set; } = false;
        public List<PlayerInventory> Inventory { get; set; } = new();
        public List<OfflineReward> OfflineRewards { get; set; } = new();

        // 스탯 계산 (Unity Component와 유사한 패턴)
        public void RecalculateStats()
        {
            var baseStats = GetBaseStatsByClass();
            var levelMultiplier = 1 + (Level - 1) * 0.1f;

            Health = (int)(baseStats.Health * levelMultiplier);
            Mana = (int)(baseStats.Mana * levelMultiplier);
            Attack = (int)(baseStats.Attack * levelMultiplier);
            Defense = (int)(baseStats.Defense * levelMultiplier);
        }

        private (int Health, int Mana, int Attack, int Defense) GetBaseStatsByClass()
        {
            return CharacterClass.ToLower() switch
            {
                "warrior" => (120, 30, 25, 20),
                "mage" => (80, 100, 30, 10),
                "archer" => (90, 50, 35, 15),
                _ => (100, 50, 20, 15)
            };
        }
    }
}