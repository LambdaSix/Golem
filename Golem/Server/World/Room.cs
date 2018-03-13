using System;
using System.Collections.Generic;
using System.Linq;
using Golem.Game.Items;
using Golem.Game.Mobiles;
using Golem.Server.Database;
using Golem.Server.Extensions;
using Newtonsoft.Json;

namespace Golem.Server.World
{
    public class Room : IStorable
    {
        public string Key { get; set; }
        public string Area { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, RoomExit> Exits { get; private set; } = new Dictionary<string, RoomExit>();
        public bool CanPractice { get; set; }
        public Dictionary<string, int> PopItems { get; private set; } = new Dictionary<string, int>();

        [JsonIgnore]
        private List<IMobile> Mobs { get; } = new List<IMobile>();

        [JsonIgnore]
        public CoinStack Money { get; } = new CoinStack(0);

        [JsonIgnore]
        public Dictionary<string, string> Items { get; } = new Dictionary<string, string>();

        [JsonIgnore]
        public Dictionary<string, DateTime> CorpseQueue { get; } = new Dictionary<string, DateTime>();

        public IEnumerable<IPlayer> Players => Mobs.OfType<IPlayer>();
        public IEnumerable<IMobile> Mobiles => Mobs;

        public void AddMobile(IMobile mobile)
        {
            if (!Mobs.Contains(mobile))
                Mobs.Add(mobile);
        }

        public void RemoveMobile(IMobile mobile) => Mobs.Remove(mobile);

        public bool HasExit(string exitName) => Exits.ContainsKey(exitName);
        public RoomExit GetExit(string exitName) => Exits[exitName];

        public RoomExit FindExitByPartial(string exitSearch) =>
            Exits.FirstOrDefault(s => s.Key.StartsWith(exitSearch)).Value;

        public RoomExit FindExitByTarget(string targetKey) =>
            Exits.FirstOrDefault(s => s.Value.LeadsTo == targetKey).Value;

        public void AddItem(ItemInstance item) => Items[item.Key] = item.Name;
        public void RemoveItem(ItemInstance item) => Items.Remove(item.Key);

        public void SendPlayers(string format, IPlayer subject, IPlayer target, params IPlayer[] ignore)
        {
            foreach (var player in Players)
            {
                if (ignore != null && ignore.Contains(player))
                    continue;

                player.Send(format, subject, target);
            }
        }

        public IPlayer LookupPlayer(IPlayer target, string keywords)
        {
            var lookupKeywords = StringHelpers.GetKeywords(keywords);

            foreach (var player in Players)
            {
                if (player == target)
                    continue;

                var possiblePlayerKeywords = new List<string>();

                possiblePlayerKeywords.AddRange(StringHelpers.GetKeywords(player.ShortDescription));

                if (target.RememberedNames.TryGetValue(player.Key, out var value))
                    possiblePlayerKeywords.AddRange(StringHelpers.GetKeywords(value));
                else
                    target.RememberedNames.Add(player.Key, player.Forename);

                var success = lookupKeywords.All(keyword => possiblePlayerKeywords.Contains(keyword));

                if (success)
                    return player;
            }

            return null;
        }

        public IMobile LookupMobile(string keywords)
        {
            var lookupKeywords = StringHelpers.GetKeywords(keywords);

            foreach (var mobile in Mobiles.Where(s => !(s is IPlayer)))
            {
                var possibleMobileKeywords = new List<string>();

                possibleMobileKeywords.AddRange(StringHelpers.GetKeywords(mobile.Description));
                possibleMobileKeywords.AddRange(mobile.Keywords);

                var success = lookupKeywords.All(keyword => possibleMobileKeywords.Contains(keyword));
                if (success)
                    return mobile;
            }

            return null;
        }

        public void RepopulateItems()
        {
            var inRoom = new Dictionary<string, int>();

            foreach (var roomItem in Items)
            {
                var item = GolemServer.Current.Database.Get<ItemInstance>(roomItem.Key);
                if (item != null)
                {
                    if (PopItems.ContainsKey(item.TemplateKey))
                    {
                        if (inRoom.ContainsKey(item.TemplateKey))
                            inRoom[item.TemplateKey]++;
                        else
                            inRoom[item.TemplateKey] = 1;
                    }
                }
            }

            foreach (var popItem in PopItems)
            {
                var numInRoom = inRoom.ContainsKey(popItem.Key) ? inRoom[popItem.Key] : 0;
                for (int i = 0; i < popItem.Value - numInRoom; i++)
                {
                    var itemToCopy = GolemServer.Current.Database.Get<ItemTemplate>(popItem.Key);
                    if (GolemServer.Current.Random.NextDouble() < itemToCopy.RepopPercent)
                    {
                        var itemInstance = new ItemInstance(itemToCopy);
                        GolemServer.Current.Database.Put(itemInstance);
                        AddItem(itemInstance);
                    }
                }
            }
        }
    }
}