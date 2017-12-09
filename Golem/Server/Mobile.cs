using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Capsicum;
using Capsicum.Interfaces;
using Golem.Game.Mobiles;
using Golem.Server.Enumerations;
using Golem.Server.Interfaces;
using Golem.Server.Session;
using Golem.Server.World;
using Newtonsoft.Json;

namespace Golem.Server
{
    public interface IEquipable
    {
        void Equip(ISession player);
        void Unequip(ISession session);
    }

    public enum WearLocation
    {
        Head,
        Shoulders,
        LeftHand,
        RightHand,
        BothHands,
        Torso,
        Arms,
        Waist,
        Legs,
        Feet,
        None,
        Container,
        Key,
        Corpse,
    }

    public interface IMobile
    {
        public string Name { get; set; }

        int BaseStrength { get; set; }
        int BaseDexterity { get; set; }
        int BaseConstitution { get; set; }
        int BaseIntelligence { get; set; }
        int BaseWisdom { get; set; }
        int BaseCharisma { get; set; }
        int BaseLuck { get; set; }
        int BaseDamRoll { get; set; }
        int BaseHitRoll { get; set; }
        int BaseHp { get; set; }
        int BaseArmor { get; set; }
        bool IsShopkeeper { get; set; }
        int HitPoints { get; set; }
        Dictionary<string, string> Inventory { get; }
        Dictionary<WearLocation, WearSlot> Equipped { get; }
        Dictionary<string, double> Skills { get; }
        int MaxHitPoints { get; }
        int Armor { get; }
        int HitRoll { get; }
        int DamRoll { get; }
        int Strength { get; }
        int Dexterity { get; }
        int Constitution { get; }
        int Intelligence { get; }
        int Wisdom { get; }
        int Charisma { get; }
        int Luck { get; }
        string HitPointDescription { get; }
    }

    public abstract class Mobile : IMobile
    {
        public string Name { get; set; }

        public int BaseStrength { get; set; }
        public int BaseDexterity { get; set; }
        public int BaseConstitution { get; set; }
        public int BaseIntelligence { get; set; }
        public int BaseWisdom { get; set; }
        public int BaseCharisma { get; set; }
        public int BaseLuck { get; set; }
        public int BaseDamRoll { get; set; }
        public int BaseHitRoll { get; set; }
        public int BaseHp { get; set; }
        public int BaseArmor { get; set; }
        public bool IsShopkeeper { get; set; }

        public int HitPoints { get; set; }

        public Dictionary<string, string> Inventory { get; protected set; } = new Dictionary<string, string>();

        public Dictionary<WearLocation, WearSlot> Equipped { get; protected set; } = new Dictionary<WearLocation, WearSlot>();

        public Dictionary<string, double> Skills { get; protected set; } = new Dictionary<string, double>();

        [JsonIgnore]
        public int MaxHitPoints
        {
            get
            {
                if (Equipped != null)
                    return BaseHp + Equipped.Sum(e => AllGear(e).HpBonus);

                return BaseHp;
            }
        }

        [JsonIgnore]
        public int Armor
        {
            get
            {
                if (Equipped != null)
                    return BaseArmor +
                           Equipped.Sum(e => AllGear(e).ArmorBonus);

                return BaseArmor;
            }
        }

        [JsonIgnore]
        public int HitRoll
        {
            get
            {
                if (Equipped != null)
                    return BaseHitRoll +
                           Equipped.Sum(e => AllGear(e).HitRoll);

                return BaseHitRoll;
            }
        }

        [JsonIgnore]
        public int DamRoll
        {
            get
            {
                if (Equipped != null)
                    return BaseDamRoll +
                           Equipped.Sum(e => AllGear(e).DamRoll);

                return BaseDamRoll;
            }
        }

        [JsonIgnore]
        public int Strength
        {
            get
            {
                if (Equipped != null)
                    return BaseStrength +
                           Equipped.Sum(e => AllGear(e).StrengthBonus);

                return BaseStrength;
            }
        }

        [JsonIgnore]
        public int Dexterity
        {
            get
            {
                if (Equipped != null)
                    return BaseDexterity +
                           Equipped.Sum(e => AllGear(e).DexterityBonus);

                return BaseDexterity;
            }
        }

        [JsonIgnore]
        public int Constitution
        {
            get
            {
                if (Equipped != null)
                    return BaseConstitution +
                           Equipped.Sum(e => AllGear(e).ConstitutionBonus);

                return BaseConstitution;
            }
        }

        [JsonIgnore]
        public int Intelligence
        {
            get
            {
                if (Equipped != null)
                    return BaseIntelligence +
                           Equipped.Sum(e => AllGear(e).IntelligenceBonus);

                return BaseIntelligence;
            }
        }

        [JsonIgnore]
        public int Wisdom
        {
            get
            {
                if (Equipped != null)
                    return BaseWisdom +
                           Equipped.Sum(e => AllGear(e).WisdomBonus);

                return BaseWisdom;
            }
        }

        [JsonIgnore]
        public int Charisma
        {
            get
            {
                if (Equipped != null)
                    return BaseCharisma +
                           Equipped.Sum(e => AllGear(e).CharismaBonus);

                return BaseCharisma;
            }
        }

        [JsonIgnore]
        public int Luck
        {
            get
            {
                if (Equipped != null)
                    return BaseLuck +
                           Equipped.Sum(e => AllGear(e).LuckBonus);

                return BaseLuck;
            }
        }

        private static InstancedItem AllGear(KeyValuePair<WearLocation, WearSlot> e)
        {
            return GolemServer.Current.Database.Get<InstancedItem>(e.Value.Key);
        }

        [JsonIgnore]
        public string HitPointDescription
        {
            get
            {
                if (HitPoints >= MaxHitPoints)
                    return "is perfectly healthy";

                var hp = HitPoints / (double)MaxHitPoints;

                if (hp >= 0.9)
                    return "has a few scratches";
                if (hp >= 0.8)
                    return "has some cuts";
                if (hp >= 0.7)
                    return "has a big gash";
                if (hp >= 0.6)
                    return "has several wounds";
                if (hp >= 0.5)
                    return "is bleeding heavily";
                if (hp >= 0.4)
                    return "is gushing blood";
                if (hp >= 0.3)
                    return "is pouring blood everywhere";
                if (hp >= 0.2)
                    return "is leaking guts";
                if (hp >= 0.1)
                    return "is bleeding from every orifice possible";
                if (hp >= 0)
                    return "is about to die";
                if (hp >= -10)
                    return "is incapacitated";

                return "is mortally wounded";
            }
        }
    }
}