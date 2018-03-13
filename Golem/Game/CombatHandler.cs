﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Golem.Game.Mobiles;
using Golem.Server;
using Golem.Server.Helpers;
using Golem.Server.World;

namespace Golem.Game
{
    public enum CombatTextType
    {
        Player,
        Group,
        KillingBlow,
        Room
    }

    class DamageActionPair
    {
        public string Singular { get; set; }
        public string Plural { get; set; }
    }

    class CombatHelper
    {
        public static DamageActionPair GetDamageAction(IMobile target, int damage)
        {
            var pair = new DamageActionPair();
            pair.Singular = "hit";
            pair.Plural = "hits";

            if (target == null)
                pair.Plural = "hits";

            if (target.HitPoints - damage < target.MaxHitPoints * 0.1)
                pair.Plural = new string[] { "annihilates", "eviscerates" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.2)
                pair.Plural = new string[] { "devastates", "destroys" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.3)
                pair.Plural = new string[] { "bludgeons", "lacerates" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.4)
                pair.Plural = new string[] { "mauls", "bashes" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.5)
                pair.Plural = new string[] { "pounds", "clobbers" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.6)
                pair.Plural = new string[] { "hits", "wallops" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.7)
                pair.Plural = new string[] { "whacks", "smacks" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.8)
                pair.Plural = new string[] { "scratches", "grazes" }[GolemServer.Current.Random.Next(2)];
            else if (target.HitPoints - damage < target.MaxHitPoints * 0.9)
                pair.Plural = new string[] { "dents", "nicks" }[GolemServer.Current.Random.Next(2)];
            else
                pair.Plural = new string[] { "nicks", "barely hits" }[GolemServer.Current.Random.Next(2)];

            pair.Singular = pair.Plural.EndsWith("es") ?
                pair.Plural.Length >= 4 && new string[] { "sh", "ch" }.Contains(pair.Plural.Substring(pair.Plural.Length - 4, 2)) ?
                pair.Plural.Substring(0, pair.Plural.Length - 2) : pair.Plural.Substring(0, pair.Plural.Length - 1) :
                pair.Plural.Substring(0, pair.Plural.Length - 1);

            return pair;
        }
    }

    public class CombatRound
    {
        private Dictionary<IPlayer, string> playerText { get; set; }
        private Dictionary<IPlayer, string> complementGroupText { get; set; }
        private Dictionary<IPlayer, string> killingBlowText { get; set; }
        private string roomText { get; set; }

        public CombatRound()
        {
            playerText = new Dictionary<IPlayer, string>();
            complementGroupText = new Dictionary<IPlayer, string>();
            killingBlowText = new Dictionary<IPlayer, string>();
        }

        public IPlayer[] PlayerKeys()
        {
            return playerText.Keys.ToArray();
        }

        public IPlayer[] GroupKeys()
        {
            return complementGroupText.Keys.ToArray();
        }

        public IPlayer[] KillingBlowKeys()
        {
            return killingBlowText.Keys.ToArray();
        }

        public string GetRoomText()
        {
            return roomText;
        }

        public void AddText(IPlayer player, string text, CombatTextType type)
        {
            switch (type)
            {
                case CombatTextType.Player:
                    if (playerText.ContainsKey(player))
                        playerText[player] += text;
                    else
                        playerText.Add(player, text);
                    break;
                case CombatTextType.Group:
                    if (complementGroupText.ContainsKey(player))
                        complementGroupText[player] += text;
                    else
                        complementGroupText.Add(player, text);
                    break;
                case CombatTextType.KillingBlow:
                    if (killingBlowText.ContainsKey(player))
                        killingBlowText[player] += text;
                    else
                        killingBlowText.Add(player, text);
                    break;
                case CombatTextType.Room:
                    roomText += text;
                    break;
            }
        }

        /// <summary>
        /// handles proper ordering of combat text for fighters only
        /// </summary>
        /// <returns>a dictionary with each relevant player's text</returns>
        public void Print(List<IPlayer> combatOrder)
        {
            var result = new Dictionary<IPlayer, string>();

            try
            {
                // add text in order: players will see their own hits in the order they actually occurred
                foreach (var player in combatOrder)
                {
                    // add player text for that player e.g. "You hit mob"
                    if (playerText.ContainsKey(player))
                    {
                        if (result.ContainsKey(player))
                            result[player] += playerText[player];
                        else
                            result[player] = playerText[player];
                    }

                    // for all other players, add this player's group text e.g. "player1 hit mob"
                    var playerName = player.Forename;
                    foreach (var otherPlayer in combatOrder.Where(p => p.Forename != playerName))
                    {
                        if (complementGroupText.ContainsKey(player))
                        {
                            if (result.ContainsKey(otherPlayer))
                                result[otherPlayer] += complementGroupText[player];
                            else
                                result[otherPlayer] = complementGroupText[player];
                        }

                        // do group kb text "player1 killed mob"
                        if (killingBlowText.ContainsKey(player))
                            result[otherPlayer] += killingBlowText[player];
                    }

                    // do player kb e.g. "you killed mob"
                    if (killingBlowText.ContainsKey(player))
                    {
                        if (player.HitPoints > 0)
                        {
                            // player killed mob
                            result[player] += "`RYou killed " +
                                              killingBlowText[player].Replace("is DEAD!!!", string.Empty).Trim() + "!!!";
                        }
                        // else don't print anything, as player has already been advised of their status
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            foreach (var player in result.Keys)
                player.Send(result[player], null);
        }

        public void Print(IPlayer player)
        {
            if (playerText.ContainsKey(player))
            {
                // do an immediate print for player
                player.Send(playerText.FirstOrDefault(p => p.Key == player).Value, null);
                playerText.Remove(player);
            }
        }

        public static CombatRound operator +(CombatRound r1, CombatRound r2)
        {
            foreach (var player in r2.playerText)
            {
                if (!string.IsNullOrWhiteSpace(player.Value))
                {
                    if (r1.playerText.ContainsKey(player.Key))
                        r1.playerText[player.Key] += player.Value;
                    else
                        r1.playerText[player.Key] = player.Value;
                }
            }

            foreach (var player in r2.complementGroupText)
            {
                if (!string.IsNullOrWhiteSpace(player.Value))
                {
                    if (r1.complementGroupText.ContainsKey(player.Key))
                        r1.complementGroupText[player.Key] += player.Value;
                    else
                        r1.complementGroupText[player.Key] = player.Value;
                }
            }

            foreach (var player in r2.killingBlowText)
            {
                if (!string.IsNullOrWhiteSpace(player.Value))
                {
                    if (r1.killingBlowText.ContainsKey(player.Key))
                        r1.killingBlowText[player.Key] += player.Value;
                    else
                        r1.killingBlowText[player.Key] = player.Value;
                }
            }

            if (!string.IsNullOrWhiteSpace(r2.roomText))
                r1.roomText += r2.roomText;

            return r1;
        }
    }

    public class Combat
    {
        private bool isAggro;
        private Room room;

        private readonly List<IPlayer> fighters = new List<IPlayer>();
        private readonly List<IMobile> mobs = new List<IMobile>();
        private readonly List<IPlayer> fightersToIgnore = new List<IPlayer>();
        private readonly List<IPlayer> combatOrder = new List<IPlayer>();
        private readonly List<IPlayer> removeFromCombatLater = new List<IPlayer>();

        public IEnumerable<IPlayer> GetFighters()
        {
            return fighters.ToList();
        }

        public IEnumerable<IMobile> GetMobs()
        {
            return mobs.ToList();
        }

        public bool Fighting => fighters.Count > 0 && mobs.Count > 0;

        public void AddFighter(IPlayer player)
        {
            if (!fighters.Contains(player))
                fighters.Add(player);
            if (!fightersToIgnore.Contains(player))
                fightersToIgnore.Add(player);
            if (!combatOrder.Contains(player))
                combatOrder.Add(player);
        }

        public void AddMob(IMobile npc)
        {
            if (!mobs.Contains(npc))
                mobs.Add(npc);
        }

        public void RemoveFromCombat(IMobile npc)
        {
            mobs.Remove(npc);
        }

        public void RemoveFromCombat(Player player)
        {
            fighters.Remove(player);
        }

        internal void Start()
        {
            if (fighters.Count < 1 || mobs.Count < 1)
                throw new Exception("Cannot start combat. 1 Player, 1 NonPlayer required.");

            foreach (var fighter in fighters)
            {
                if (room == null)
                    room = RoomHelper.GetPlayerRoom(fighter.Location);

                fighter.Status = MobileStatus.Fighting;
            }

            foreach (var mob in mobs)
            {
                if (room == null)
                    room = RoomHelper.GetPlayerRoom(mob.Location);

                // if at least one mob is aggro, the whole fight is 'aggro' i.e. mob gets first hit each round
                if (mob.Aggro)
                    isAggro = true;

                mob.Status = MobileStatus.Fighting;
            }
        }

        internal void Round(long combatTickRate)
        {
            var roundText = new CombatRound();

            if (isAggro)
            {
                roundText += DoMobHits();

                roundText += DoPlayerHits();
            }
            else
            {
                roundText += DoPlayerHits();

                roundText += DoMobHits();
            }

            roundText.Print(combatOrder);

            foreach (var removing in removeFromCombatLater)
                combatOrder.Remove(removing);

            // send text to room
            room.SendPlayers(roundText.GetRoomText(), null, null, fightersToIgnore.ToArray());

            // no longer ignore incapacitated players
            foreach (var removing in removeFromCombatLater)
            {
                fightersToIgnore.Remove(removing);

                if (removing.HitPoints < ServerConstants.DeadHitpoints)
                    removing.DieForReal();
            }

            // reset each round
            removeFromCombatLater.Clear();

            Thread.Sleep((int)combatTickRate);
        }

        internal void End()
        {
            foreach (var fighter in fighters)
                fighter.Status = MobileStatus.Standing;

            foreach (var mob in mobs)
                mob.Status = MobileStatus.Standing;
        }

        private CombatRound DoMobHits()
        {
            //Console.WriteLine("Enter DoMobHits");
            var round = new CombatRound();

            try
            {
                // mob hits first
                foreach (var npc in mobs)
                {
                    // only attempt to hit if there are players left to hit
                    if (fighters.Any(p => p.HitPoints > 0))
                    {
                        // choose player to hit at random, and hit
                        var playerToHit = fighters
                            .Where(p => p.HitPoints > 0)
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                        round += npc.Hit(playerToHit); // queues "mob hit you" and "mob hit <player>" text

                        if (playerToHit.HitPoints <= 0)
                        {
                            playerToHit.Die(); // changes status

                            var statusText = Player.GetStatusText(playerToHit.Status).ToUpper();

                            var playerText = string.Format("You are {0}!!!", statusText);
                            if (playerToHit.HitPoints < ServerConstants.DeadHitpoints)
                                playerText += " You have respawned, but you're in a different location.\n" +
                                              "Your corpse will remain for a short while, but you'll want to retrieve your\n" +
                                              "items in short order.";

                            var groupText = string.Format("{0} is {1}!!!", playerToHit.Forename, statusText);

                            round.AddText(playerToHit, playerText, CombatTextType.Player);
                            round.AddText(playerToHit, groupText, CombatTextType.Group);
                            round.AddText(playerToHit, groupText, CombatTextType.Room);

                            RemoveFromCombat(playerToHit);
                            removeFromCombatLater.Add(playerToHit);

                            if (fighters.Count == 0)
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return round;
        }

        private CombatRound DoPlayerHits()
        {
            //Console.WriteLine("Enter DoPlayerHits");
            var round = new CombatRound();

            try
            {
                var killedBy = new Dictionary<IMobile, IPlayer>();

                // if still players, they hit
                foreach (var player in fighters)
                {
                    if (mobs.Any(m => m.HitPoints > 0))
                    {
                        var mobToHit = mobs
                            .Where(m => m.HitPoints > 0)
                            .OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                        round += player.Hit(mobToHit);

                        // check for killing blow, could be multiple
                        if (mobToHit.HitPoints <= 0)
                        {
                            killedBy.Add(mobToHit, player);
                            // remove mob from combat, so it can't be hit any more
                            RemoveFromCombat(mobToHit);
                            round += mobToHit.Die();
                            if (mobs.Count == 0)
                                break; // stop fighting
                        }
                    }
                }

                if (killedBy.Count > 0)
                {
                    foreach (var kb in killedBy)
                    {
                        round.AddText(kb.Value, string.Format("`R{0} is DEAD!!!\n", kb.Key.Name), CombatTextType.KillingBlow);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return round;
        }
    }

    /// <summary>
    /// main handler of combat
    /// </summary>
    public class CombatHandler
    {
        private List<Combat> Fights { get; set; }
        private System.Timers.Timer _timer { get; set; }
        private long _combatTickRate;

        public CombatHandler(long combatTickRate)
        {
            Fights = new List<Combat>();
            _combatTickRate = combatTickRate;
            _timer = new System.Timers.Timer(_combatTickRate);
            _timer.Elapsed += DoCombat;
        }

        public void StartFight(Combat combat)
        {
            Fights.Add(combat);
            combat.Start();
        }

        public void EnterRoom(IPlayer player, Room room)
        {
            // if player is first in room, check npc's for aggro
            if (room.Mobiles.Count() == 1)
            {
                foreach (var npc in room.Mobiles)
                {
                    if (npc.Aggro)
                    {
                        var combat = new Combat();
                        combat.AddFighter(player);
                        combat.AddMob(npc);

                        // add any other aggro npc's
                        var npcName = npc.Name;
                        foreach (var otherNpc in room.Mobiles.Where(n => n.Name != npcName))
                            combat.AddMob(npc);

                        StartFight(combat);
                        break;
                    }
                }
            }
        }

        private void DoCombat(object sender, ElapsedEventArgs e)
        {
            //Server.Current.Log("DoCombat");
            foreach (var combat in Fights.ToArray())
            {
                try
                {
                    if (combat.Fighting) // could've ended on another thread
                        combat.Round(_combatTickRate);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // end fight
                if (!combat.Fighting)
                {
                    combat.End();
                    Fights.Remove(combat);
                }
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void AddToCombat(IPlayer player, string mobKey)
        {
            foreach (var fight in Fights)
                if (fight.GetMobs().Any(m => m.Key == mobKey))
                    fight.AddFighter(player);
        }

        public Combat FindFight(IPlayer player)
        {
            return Fights.FirstOrDefault(f => f.GetFighters().Contains(player));
        }

        public Combat FindFight(IMobile npc)
        {
            return Fights.FirstOrDefault(f => f.GetMobs().Contains(npc));
        }
    }
}