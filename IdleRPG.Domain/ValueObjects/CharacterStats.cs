namespace IdleRPG.Domain.ValueObjects
{
    public class CharacterStats
    {
        public int Strength { get; }
        public int Dexterity { get; }
        public int Intelligence { get; }
        public int Vitality { get; }

        public CharacterStats(int strength, int dexterity, int intelligence, int vitality)
        {
            if (strength < 1) throw new ArgumentException("Strength must be at least 1", nameof(strength));
            if (dexterity < 1) throw new ArgumentException("Dexterity must be at least 1", nameof(dexterity));
            if (intelligence < 1) throw new ArgumentException("Intelligence must be at least 1", nameof(intelligence));
            if (vitality < 1) throw new ArgumentException("Vitality must be at least 1", nameof(vitality));

            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Vitality = vitality;
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
            return Strength == other.Strength &&
                   Dexterity == other.Dexterity &&
                   Intelligence == other.Intelligence &&
                   Vitality == other.Vitality;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Strength, Dexterity, Intelligence, Vitality);
        }
    }
}