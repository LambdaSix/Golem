using System;
using System.Collections.Generic;
using System.Linq;
using Golem.Game.Mobiles;
using Golem.Server.Database;
using Golem.Server.Helpers;
using Golem.Server.Session;
using Newtonsoft.Json;

namespace Golem.Server
{
    public class InstancedItem : IStorable, IEquipable
    {
        private Guid _guid;
        
        public string Key => _guid.ToString();

        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Keywords { get; set; }
        public int Weight { get; set; }
        public int Value { get; set; }
        public WearLocation WearLocation { get; set; }
        public bool CanPull { get; set; }

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

        // TODO: Convert Gold from an int to a proper Gold that auto-sorts currencies.
        public int Gold { get; set; }

        [JsonIgnore]
        public string TemplateKey { get; set; }

        [JsonIgnore]
        public string AllowedToLoot { get; set; }

        [JsonIgnore]
        public int ContainerWeight
        {
            get
            {
                // If we're not a container, don't sweat it.
                if (WearLocation != WearLocation.Container)
                    return Weight;

                // Take our own weight.
                int weight = Weight;
                foreach (var key in ContainedItems.Keys)
                {
                    var item = GolemServer.Current.Database.Get<InstancedItem>(key);
                    if (item.WearLocation == WearLocation.Container) // Another container, recursive maaan
                        weight += item.ContainerWeight;
                    else
                        weight += item.Weight;
                }

                return weight;
            }
        }

        public InstancedItem(PrototypeItem prototype)
        {
            Name = prototype.Name;
            Description = prototype.Description;
            Keywords = prototype.Keywords;
            Weight = prototype.Weight;
            Value = prototype.Value;
            WearLocation = prototype.WearLocation;
            CanPull = prototype.CanPull;

            HpBonus = prototype.HpBonus;
            ArmorBonus = prototype.ArmorBonus;
            DamRoll = prototype.DamRoll;
            HitRoll = prototype.HitRoll;
            ContainedItems = prototype.ContainedItems;

            StrengthBonus = prototype.StrengthBonus;
            DexterityBonus = prototype.DexterityBonus;
            ConstitutionBonus = prototype.ConstitutionBonus;
            IntelligenceBonus = prototype.IntelligenceBonus;
            WisdomBonus = prototype.WisdomBonus;
            LuckBonus = prototype.LuckBonus;
        }

        public void LookAt(ISession session)
        {
            session.WriteLine(Description);

            if (WearLocation == WearLocation.Container || WearLocation == WearLocation.Corpse)
            {
                if (ContainedItems.Count == 0 && Gold <= 0)
                {
                    session.WriteLine("\tEmpty");
                    return;
                }

                if (Gold > 0)
                    session.WriteLine($"\t{Gold} gold coin{(Gold > 1 ? "s" : "")}");

                foreach (var itemLine in ContainedItems
                    .GroupBy(i => i.Key)
                    .Select(group => new
                    {
                        ItemName = group.Key,
                        Count = group.Count()
                    }))
                {
                    session.WriteLine($"\t{itemLine.ItemName} ({itemLine.Count})");
                }
            }
        }

        /// <inheritdoc />
        public void Equip(ISession session)
        {
            if (WearLocation == WearLocation.None || WearLocation == WearLocation.Key ||
                WearLocation == WearLocation.Container)
            {
                session.WriteLine("You can't equip that.");
                return;
            }

            if (WearingThingOn(session, WearLocation))
            {
                session.WriteLine("You've already equipped something in that slot.");
                return;
            }

            if (WearLocation == WearLocation.BothHands &&
                WearingThingOn(session, WearLocation.LeftHand, WearLocation.RightHand))
            {
                session.WriteLine("You can't hold a two-handed weapon with any other weapon");
                return;
            }

            if ((WearLocation == WearLocation.RightHand || WearLocation == WearLocation.LeftHand)
                && WearingThingOn(session, WearLocation.BothHands))
            {
                session.WriteLine("You're already using both hands");
                return;
            }

            session.Player.Equipped[WearLocation] = new WearSlot
            {
                Key = Key,
                Name = Name
            };

            session.Player.Inventory.Remove(Key);

            session.WriteLine($"You equip {Name}");
            RoomHelper.GetPlayerRoom(session.Player)
                .SendPlayers($"%d dons {Name}", session.Player, null, session.Player);
        }

        private bool WearingThingOn(ISession session, params WearLocation[] locations)
        {
            foreach (var location in locations)
            {
                if (session.Player.Equipped.ContainsKey(location))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Unequip(ISession session)
        {
            session.Player.Equipped.Remove(WearLocation);
            session.Player.Inventory[Key] = Name;

            session.WriteLine("You remove {0}", Name);
            RoomHelper.GetPlayerRoom(session.Player).SendPlayers(string.Format("%d removes {0}", Name), session.Player, null, session.Player);
        }
    }
}