
using System;

namespace IdleRPG.Domain.Entities
{
    public class GachaHistory
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public int SkillTemplateId { get; set; }
        public bool WasPity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Character Character { get; set; }
        public virtual SkillTemplate SkillTemplate { get; set; }
    }
}
