namespace IdleRPG.Application.DTOs.Characters
{
    public class CharacterDto
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; } 
        public int Level { get; set; }
        public long Experience { get; set; }
        public int StatPoints { get; set; }

        // 평탄화된 스탯 정보
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Vitality { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}