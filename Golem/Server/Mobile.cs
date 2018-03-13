using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;
using Golem.Game;
using Golem.Game.Items;
using Golem.Game.Mobiles;
using Golem.Server.Database;
using Golem.Server.Helpers;
using Golem.Server.Session;
using Golem.Server.World;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public interface IMobile : IStorable
    {
        string Name { get; set; }
        string Description { get; set; }

        
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
        IEnumerable<string> Keywords { get; set; }
        string Location { get; set; }
        bool Aggro { get; set; }
        MobileStatus Status { get; set; }
        CombatRound Hit(IMobile player);
        void Die();
    }

    public abstract class Mobile : IMobile
    {
        public string Key => Name.ToLower();

        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Keywords { get; set; }

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

        public MobileStatus Status { get; set; }
        public string Location { get; set; }

        public bool Aggro { get; set; }

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

        private static ItemInstance AllGear(KeyValuePair<WearLocation, WearSlot> e)
        {
            return GolemServer.Current.Database.Get<ItemInstance>(e.Value.Key);
        }
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

        public virtual CombatRound Hit(IPlayer player)
        {
            return null;
        }

        public void Die()
        {
            if (HitPoints < ServerConstants.DeadHitpoints)
            {
                Status = HitPoints >= ServerConstants.IncapacitatedHitpoints
                    ? MobileStatus.Incapacitated
                    : MobileStatus.MortallyWounded;
            }
            else
            {
                Status = MobileStatus.Dead;
            }
        }
    }

    public class MobTemplate : Mobile
    {
        public string[] RespawnRoom { get; set; }
        public string[] Phrases { get; set; }
        public double TalkProbability { get; set; }
        public long MinimumTalkInterval { get; set; }

        public List<string> AllowedRooms { get; set; } = new List<string>();
        public new List<string> Inventory { get; private set; } = new List<string>();
        public new Dictionary<WearLocation, string> Equipped { get; private set; } = new Dictionary<WearLocation, string>();
        public new Dictionary<string, Tuple<double,double>> Skills { get; private set; } = new Dictionary<string, Tuple<double, double>>();

        public int CopperPeices { get; set; }
        public int MaxCopperPieces { get; set; }
        public int MinCopperPieces { get; set; }
    }

    public class MobInstance : Mobile
    {
        private Guid _guid;
        private DateTime _lastTimeTalked;
        private DateTime _lastTimeWalked;
        private bool _skillLoopStarted;
        private bool _skillReady;

        public string Key => _guid.ToString();

        public string MobTemplateKey { get; set; }
        public string[] RespawnRoom { get; set; }
        public string[] Phrases { get; set; }
        public double TalkProbability { get; set; }
        public long MinimumTalkInterval { get; set; }
        public bool Aggro { get; set; }
        public List<string> AllowedRooms { get; set; }
        public new Dictionary<string, Tuple<double,double>> Skills { get; set; }

        public CoinStack Gold { get; set; }
        public CoinStack MaxGold { get; set; }
        public CoinStack MinGold { get; set; }

        [JsonIgnore]
        public bool DoesWander => AllowedRooms != null && AllowedRooms.Count > 1;

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="template"></param>
        public MobInstance(MobTemplate template)
        {
            _guid = new Guid(template.Key);

            Name = template.Name;
            MobTemplateKey = template.Key;
            Status = template.Status;
            Keywords = template.Keywords;
            Description = template.Description;
            RespawnRoom = template.RespawnRoom;
            Location = template.Location;
            Phrases = template.Phrases;
            TalkProbability = template.TalkProbability;
            MinimumTalkInterval = template.MinimumTalkInterval;
            HitPoints = template.HitPoints;
            Aggro = template.Aggro;
            BaseArmor = template.BaseArmor;
            BaseHitRoll = template.BaseHitRoll;
            BaseDamRoll = template.BaseDamRoll;
            AllowedRooms = template.AllowedRooms ?? new List<string>();
            Inventory = MapInventory(template.Inventory).AsDictionary() ?? new Dictionary<string, string>();
            Equipped = MapEquipped(template.Equipped).AsDictionary() ?? new Dictionary<WearLocation, WearSlot>();
            Skills = Skills ?? new Dictionary<string, Tuple<double, double>>();

            _skillReady = true;
            _lastTimeTalked = DateTime.Now;
            _lastTimeWalked = DateTime.Now;
            IsShopkeeper = template.IsShopkeeper;

            Gold = new CoinStack(template.CopperPeices);
            MinGold = new CoinStack(template.MinCopperPieces);
            MaxGold = new CoinStack(template.MaxCopperPieces);
        }

        public MobInstance()
        {
            var guid = Guid.NewGuid();
            while (GolemServer.Current.Database.Exists<MobInstance>(guid.ToString()))
            {
                guid = Guid.NewGuid();
            }

            _guid = guid;
            _lastTimeTalked = DateTime.Now;
            _lastTimeWalked = DateTime.Now;
        }

        public string GetRespawnRoom()
        {
            if (RespawnRoom == null || RespawnRoom.Length == 0)
                return String.Empty;

            if (RespawnRoom != null && RespawnRoom.Length > 1)
                return RespawnRoom[GolemServer.Current.Random.Next(0, RespawnRoom.Length)];

            return RespawnRoom[0];
        }

        private CoinStack GetRandomGold() => new CoinStack(GolemServer.Current.Random.Next((int)MinGold.RawValue, (int)MaxGold.RawValue));

        public override CombatRound Hit(IPlayer player)
        {
            var round = new CombatRound();

            if (GolemServer.Current.Random.Next(HitRoll) + 1 >= player.Armor)
            {
                // register a hit
                var damage = GolemServer.Current.Random.Next(DamRoll) + 1;
                player.HitPoints -= damage;

                var damageAction = CombatHelper.GetDamageAction(player, damage);

                var playerText = $"{Name} {damageAction.Plural} you for {damage} damage!";
                round.AddText(player, playerText, CombatTextType.Player);

                var groupText = $"{Name} {damageAction.Plural} {player.Forename}!";
                round.AddText(player, groupText, CombatTextType.Group);
            }
            else
            {
                var playerText = $"{Name} misses you!";
                round.AddText(player, playerText, CombatTextType.Player);

                var groupText = $"{Name} misses {player.Forename}";
                round.AddText(player, groupText, CombatTextType.Group);
            }

            if (!_skillLoopStarted && Skills.Any())
                DoSkillLoop(null, null);

            return round;
        }

        private void DoSkillLoop(object sender, ElapsedEventArgs e)
        {
            _skillLoopStarted = true;
            _skillReady = true;

            if (HitPoints > 0) // only use skills when alive
            {
                var keys = new List<string>(Skills.Keys);
                var size = Skills.Count;
                var skillKey = keys[GolemServer.Current.Random.Next(size)];
                var skill = GolemServer.Current.CombatSkills.FirstOrDefault(s => s.Key == skillKey);
                var command = GolemServer.Current.CommandLookup.FindCommand(skillKey, true);

                if (skill == null || command == null)
                {
                    GolemServer.Current.Log($"Can't find NPC Skill: {skillKey}");
                    return;
                }
                var frequency = Skills[skillKey].Item1;
                var effectiveness = Skills[skillKey].Item2;

                if (GolemServer.Current.Random.NextDouble() < frequency)
                {
                    var fight = GolemServer.Current.CombatHandler.FindFight(this);

                    if (fight == null)
                        return;

                    IPlayer playerToHit = fight.GetRandomFighter();

                    if (playerToHit == null)
                        return;

                    var room = RoomHelper.GetPlayerRoom(playerToHit.Location);

                    if (GolemServer.Current.Random.NextDouble() < effectiveness)
                    {
                        var damage = GolemServer.Current.Random.Next(skill.MinDamage, skill.MaxDamage + 1);
                        var damageAction = CombatHelper.GetDamageAction(playerToHit, damage);

                        playerToHit.Send(
                            $"{Name}{(Name.EndsWith("s") ? "'" : "'s")} {skillKey.ToLower()} {damageAction.Plural} you for {damage} damage!",
                            null);
                        playerToHit.HitPoints -= damage;

                        room.SendPlayers(
                            $"{Name}{(Name.EndsWith("s") ? "'" : "'s")} {skillKey.ToLower()} {damageAction.Plural} {playerToHit.Forename}",
                            playerToHit, null, playerToHit);

                        // Check if the player died
                        if (playerToHit.HitPoints <= 0)
                        {
                            playerToHit.Die(); // set status

                            var statusText = Player.GetStatusText(playerToHit.Status).ToUpper();
                            var playerText = $"You are {statusText}!";
                            if (playerToHit.HitPoints < ServerConstants.DeadHitpoints)
                            {

                                playerText += " You have respawned, but you're in a different location.\n" +
                                              "Your corpse will remain for a short while, but you'll want to retrieve your\n" +
                                              "items in short order.";

                                playerToHit.DieForReal();
                            }

                            var groupText = $"{playerToHit.Forename} is {statusText}!";

                            playerToHit.Send(playerText, null);
                            room.SendPlayers(groupText, playerToHit, null, playerToHit);

                            fight.RemoveFromCombat(playerToHit);

                            if (!fight.GetFighters().Any())
                            {
                                fight.End();
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Miss
                        playerToHit.Send($"{Name}{(Name.EndsWith("s") ? "'" : "'s")} {skillKey.ToLower()} misses you!",
                            null);
                        room.SendPlayers(
                            $"{Name}{(skillKey.EndsWith("s") ? "'" : "'s")} {skillKey.ToLower()} misses {playerToHit.Forename}",
                            playerToHit, null, playerToHit);
                    }
                }

                _skillReady = false;

                // set delay and call this method again
                var t = new Timer()
                {
                    AutoReset = false,
                    Interval = (long)command.TickLength,
                };

                t.Elapsed += DoSkillLoop;
                t.Start();
            }
        }

        public void TalkOrWalk()
        {
            if (Phrases != null && Phrases.Length > 0
                                && AllowedRooms != null && AllowedRooms.Count > 1)
            {
                if (GolemServer.Current.Random.Next(2) == 0)
                    Talk();
                else
                    Walk();
            }
            else if (Phrases != null && Phrases.Length > 0)
                Talk();
            else if (AllowedRooms != null && AllowedRooms.Count > 1)
                Walk();
        }

        protected void Talk()
        {
            if ((DateTime.Now - _lastTimeTalked).TotalMilliseconds <= MinimumTalkInterval)
                return;

            // set the new interval
            _lastTimeTalked = DateTime.Now;

            // talk at random
            double prob = GolemServer.Current.Random.NextDouble();
            if (prob < TalkProbability && Phrases != null && Phrases.Length > 0)
            {
                var phrase = Phrases[GolemServer.Current.Random.Next(Phrases.Length)];

                // say it to the room
                var room = RoomHelper.GetPlayerRoom(Location);
                if (room != null)
                {
                    string message = string.Format("{0} says, \"{1}\"", Name, phrase);
                    room.SendPlayers(message, null, null, null);
                }
            }
        }

        protected void Walk()
        {
            if ((DateTime.Now - _lastTimeWalked).TotalMilliseconds <= ServerConstants.MobWalkInterval)
                return;

            _lastTimeWalked = DateTime.Now;

            var room = RoomHelper.GetPlayerRoom(Location);

            // get allowed exits
            var allowedExits = room.Exits.Where(e => AllowedRooms.Contains(e.Value.LeadsTo) && e.Value.IsOpen).ToList();

            if (allowedExits.Any() && GolemServer.Current.Random.NextDouble() < 0.5)
            {
                var exit = allowedExits.Skip(GolemServer.Current.Random.Next(allowedExits.Count())).FirstOrDefault();

                room.RemoveMobile(this);
                var newRoom = RoomHelper.GetPlayerRoom(exit.Value.LeadsTo);
                newRoom.AddMobile(this);
                Location = newRoom.Key;
                room.SendPlayers(string.Format("{0} heads {1}.", Name, DirectionHelper.GetDirectionWord(exit.Key)),
                                    null, null, null);
                newRoom.SendPlayers(
                    string.Format("{0} arrives from the {1}.", Name, DirectionHelper.GetOppositeDirection(exit.Key)),
                    null, null, null);
            }
        }

        private IEnumerable<KeyValuePair<string,string>> MapInventory(IEnumerable<string> inventory)
        {
            foreach (var key in inventory)
            {
                var itemTemplate = GolemServer.Current.Database.Get<ItemTemplate>(key);
                var item = new ItemInstance(itemTemplate);
                GolemServer.Current.Database.Save(item);
                yield return new KeyValuePair<string, string>(item.Key, item.Name);
            }
        }

        private IEnumerable<KeyValuePair<WearLocation, WearSlot>> MapEquipped(
            Dictionary<WearLocation, string> equipment)
        {
            foreach (var piece in equipment)
            {
                var itemTemplate = GolemServer.Current.Database.Get<ItemTemplate>(piece.Value);
                var item = new ItemInstance(itemTemplate);
                GolemServer.Current.Database.Save(item);
                yield return new KeyValuePair<WearLocation, WearSlot>(piece.Key, new WearSlot()
                {
                    Key = item.Key,
                    Name = item.Name
                });
            }
        }
    }

    public static class KeyValuePairExtensions
    {
        public static Dictionary<TKey, TValue> AsDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> sequence)
        {
            return sequence.ToDictionary(key => key.Key, value => value.Value);
        }
    }
}