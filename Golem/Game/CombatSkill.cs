using Golem.Server.Database;

namespace Golem.Game
{
    public class CombatSkill : IStorable
    {
        public string Key => Name.ToLower();

        public string Name { get; set; }
        public double Effectiveness { get; set; }
        public double MaxEffectiveness { get; set; }
        public double MissEffectivenessIncrease { get; set; }
        public double HitEffectivenessIncrease { get; set; }
        public double KillingBlowEffectivenessIncrease { get; set; }
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
    }
}