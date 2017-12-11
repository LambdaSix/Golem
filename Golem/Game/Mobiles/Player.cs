using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Golem.Server;
using Golem.Server.Database;
using Golem.Server.Extensions;
using Golem.Server.Text;
using Newtonsoft.Json;

namespace Golem.Game.Mobiles
{
    public enum PlayerGender
    {
        Male,
        Female,
        Neuter,
    }

    public enum PlayerPronouns
    {
        Feminine, // She/her
        Masculine, // He/him
        Neuter, // Their/Them
    }

    public enum MobileStatus
    {
        Sitting,
        Standing,
        Sleeping,
        Fighting,
        Incapacitated, // something like >= -3 hp
        MortallyWounded, // will die if unaided
        Dead,
        Trade,
    }

    public struct WearSlot
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }

    public class Player : Mobile, IStorable, IPlayer
    {
        string _passwordHash;
        private int _weight;
        private int _experience;

        [JsonIgnore]
        public string Key => Forename.ToLower();

        public string PasswordHash { get => _passwordHash; set => _passwordHash = Hash(value); }

        public Dictionary<string, string> RememberedNames { get; } = new Dictionary<string, string>();

        public string Forename { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public object Location { get; set; }
        public bool Approved { get; set; }
        public PlayerGender Gender { get; set; }
        public PlayerPronouns Pronouns { get; set; }
        public string Prompt { get; set; }
        public object RespawnRoom { get; set; }
        public MobileStatus Status { get; set; }

        public bool IsAdmin { get; set; }
        public int Level { get; set; }
        public int MinutesPlayed { get; set; }
        public int SkillPoints { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int Experience { get => _experience; set => SetExperience(value); }

        /// <inheritdoc />
        [JsonIgnore]
        public int Weight => GetWeight();

        /// <inheritdoc />
        [JsonIgnore]
        public int MaxInventory => Strength <= 10 ? 10 : Strength + 2;

        /// <inheritdoc />
        [JsonIgnore]
        public int MaxWeight => Strength <= 10 ? 100 : (Strength * 50) - 400;

        /// <inheritdoc />
        [JsonIgnore]
        public bool LoggedIn { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int Age => MinutesPlayed / 120 + 1;
        
        [JsonIgnore]
        public IOutputTextWriter OutputWriter { get; set; }

        public Player() :base()
        {
            
        }

        public void SetOutputWriter(IOutputTextWriter writer)
        {
            OutputWriter = writer;
        }

        public bool CheckPassword(string password)
        {
            var hash = Hash(password);

            return PasswordHash == hash;
        }

        public bool ChangePassword(string newPassword)
        {
            var hash = Hash(newPassword);
            if (hash == PasswordHash)
            {
                return false;
            }
            else
            {
                PasswordHash = hash;
                return true;
            }
        }

        public string GetOtherPlayerDescription(IPlayer subject)
        {
            if (subject == this)
                return this.Forename;

            if (RememberedNames.ContainsKey(subject.Key))
                return RememberedNames[subject.Key];

            return subject.ShortDescription;
        }

        public void Send(string format, Player subject)
        {
            Send(format, subject, null);
        }

        public void Send(string format, Player subject, Player target)
        {
            OutputWriter.WriteLine(StringHelpers.BuildString(format, this, subject, target));
        }

        public void WritePrompt()
        {
            OutputWriter.WriteLine("`w<`GHP: {0}`g/`G{1} `Y${2:N0}`w>", HitPoints, MaxHitPoints, GetGold());
        }

        public static string NameToKey(string name) => name.ToLower();

        public CombatRound Hit(IMobile mob)
        {
            // TODO: Move this into a proper combat class I guess

            var round = new CombatRound();

            if (GolemServer.Current.Random.Next(HitRoll) + 1 >= mob.Armor)
            {
                // Hit!
                var damage = GolemServer.Current.Random.Next(DamRoll) + 1;
                mob.HitPoints -= damage;

                // Apply a few XP points for the damage done.
                ExperienceHelper.ApplyExperience(this, damage);

                var damageAction = CombatHelper.GetDamageAction(mob, damage);

                var playerText = $"You {damageAction.Singular} {mob.Name} for {damage} damage!\n";
                round.AddText(this, playerText, CombatTextType.Player);

                var groupText = $"{Forename} {damageAction.Plural} {mob.Name}";
                round.AddText(this, groupText, CombatTextType.Group);
            }
            else
            {
                var playerText = $"You miss {mob.Name}!\n";
                round.AddText(this, playerText, CombatTextType.Player);

                var groupText = $"{Forename} misses {mob.Name}";
                round.AddText(this, groupText, CombatTextType.Group);
            }

            round.AddText(null, $"{Forename} is fighting {mob.Name}!", CombatTextType.Room);

            return round;
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

        public void GenerateCorpse()
        {
            // Sit the character down
            Status = MobileStatus.Sitting;
            HitPoints = 1;

            var deathRoom = RoomHelper.GetPlayerRoom(Location);
            deathRoom.RemovePlayer(this);

            // Create a corpse
            var corpsePrototype = GolemServer.Current.Database.Get<PrototypeItem>("Corpse");
            var realCorpse = new InstancedItem(corpsePrototype);
            var corpseName = $"The corpse of {Forename}";

            realCorpse.AllowedToLoot = Key;
            realCorpse.Name = corpseName;
            realCorpse.Description = corpseName;
            realCorpse.Keywords = new[] {"corpse", Forename};
            realCorpse.WearLocation = WearLocation.Corpse;

            // add the corpse to the room's corpse queue to decay after a time period.
            deathRoom.CorpseQueue[realCorpse.Key] = DateTime.Now.AddMilliseconds(ServerConstants.CorpseDecayTimeMs);

            foreach (var item in Inventory
                .Select(x => new {x.Key, x.Value})
                .Union(Equipped.Values.Select(x => new {x.Key, Value = x.Name})
                    .ToArray()))
            {
                realCorpse.ContainedItems.Add(item.Key, item.Value);
            }

            Inventory.Clear();
            Equipped.Clear();

            // Cache but don't save it.
            GolemServer.Current.Database.Put(realCorpse);
            deathRoom.AddItem(realCorpse);

            var room = RoomHelper.GetPlayerRoom(RespawnRoom);
            Location = RespawnRoom;
            room.AddPlayer(this);
        }

        public string Whois()
        {
            var result = new StringBuilder();
            var pronouns = (Pronouns == PlayerPronouns.Masculine
                ? "He is"
                : Pronouns == PlayerPronouns.Feminine
                    ? "She is"
                    : "They are");

            result.AppendLine($"{Forename} is a {Age} year old, level {Level} player");
            result.AppendLine(
                $"{pronouns} currently in {RoomHelper.PlayerRoom(Location).Title}");

            return result.ToString();
        }

        public static string GetStatusText(MobileStatus status)
        {
            switch (status)
            {
                case MobileStatus.Dead:
                    return "Dead";
                case MobileStatus.Fighting:
                    return "Fighting";
                case MobileStatus.Incapacitated:
                    return "Incapacitated";
                case MobileStatus.MortallyWounded:
                    return "Mortally wounded";
                case MobileStatus.Sitting:
                    return "Sitting";
                case MobileStatus.Sleeping:
                    return "Sleeping";
                case MobileStatus.Standing:
                    return "Standing";
                default:
                    return string.Empty;
            }
        }

        private void SetExperience(int experience)
        {
            // If they've max level, then they can't have any more experience :)
            if (Level == ExperienceHelper.LevelMax)
                return;

            _experience = experience;
            if (ExperienceHelper.CanLevelUp(Level, _experience))
                ExperienceHelper.LevelUp(this);
        }

        private int GetGold()
        {
            throw new NotImplementedException();
        }

        private int GetWeight()
        {
            _weight = 0;

            foreach (var key in Inventory.Keys.Union(Equipped.Values.Select(w => w.Key)))
            {
                var item = GolemServer.Current.Database.Get<InstancedItem>(key);
                if (item != null)
                {
                    if (item.WearLocation == WearLocation.Container)
                        _weight += item.ContainerWeight;
                    else
                        _weight += item.Weight;
                }
            }

            return _weight;
        }

        private static string Hash(string value)
        {
            using (SHA256 hashMethod = SHA256.Create())
            {
                var toHash = Encoding.ASCII.GetBytes(value);
                var hashedBytes = hashMethod.ComputeHash(toHash);
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    public interface IPlayer : IStorable
    {
        string Forename { get; set; }
        string PasswordHash { get; set; }
        string ShortDescription { get; set; }
        string Description { get; set; }
        object Location { get; set; }
        bool Approved { get; set; }
        PlayerGender Gender { get; set; }
        PlayerPronouns Pronouns { get; set; }
        string Prompt { get; set; }
        object RespawnRoom { get; set; }
        MobileStatus Status { get; set; }

        bool IsAdmin { get; set; }
        int Level { get; set; }
        int MinutesPlayed { get; set; }
        int SkillPoints { get; set; }

        [JsonIgnore]
        int Experience { get; set; }
        [JsonIgnore]
        int Weight { get; }
        [JsonIgnore]
        int MaxInventory { get; }
        [JsonIgnore]
        int MaxWeight { get; }
        [JsonIgnore]
        bool LoggedIn { get; set; }
        [JsonIgnore]
        int Age { get; }

        string GetOtherPlayerDescription(IPlayer subject);
        bool CheckPassword(string password);
        void SetOutputWriter(IOutputTextWriter writer);
    }
}