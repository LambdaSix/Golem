using System.Collections.Generic;
using Golem.Server.Database;

namespace Golem.Server
{
    public class ItemTemplate : IStorable
    {
        /// <inheritdoc />
        public string Key => Name.ToLower();

        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
        public int Weight { get; set; }
        public int Value { get; set; }
        public WearLocation WearLocation { get; set; }
        public bool CanPull { get; set; }
        public double RepopPercent { get; set; }

        // in-game item attributes
        public int HpBonus { get; set; }
        public int ArmorBonus { get; set; }
        public int DamRoll { get; set; }
        public int HitRoll { get; set; }
        public Dictionary<string, string> ContainedItems { get; set; }

        public int StrengthBonus { get; set; }
        public int DexterityBonus { get; set; }
        public int ConstitutionBonus { get; set; }
        public int IntelligenceBonus { get; set; }
        public int WisdomBonus { get; set; }
        public int CharismaBonus { get; set; }
        public int LuckBonus { get; set; }        
    }
}