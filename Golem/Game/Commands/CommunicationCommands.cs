using System;
using System.Collections.Generic;
using System.Linq;
using Capsicum;
using Golem.Game.Mobiles;
using Golem.Server.Enumerations;
using Golem.Server.Mobiles;
using Golem.Server.Session;

namespace Golem.Game.Commands
{
    [CommandContainer]
    public class CommunicationCommands
    {
        [CommandHandler("say", "[someone] (something)", "Talk to people in the current room.", AccessLevel.Player, true, true)]
        public bool DoSay(Entity actor, IEnumerable<string> args)
        {
            if (!actor.HasComponent<PostureComponent>() || !actor.HasComponent<NetworkStateComponent>())
            {
                throw new ArgumentException(
                    "Unable to process command 'Say' for entity, requires components: PostureComponent, NetworkStateComponent");
            }

            if (actor.GetComponent<PostureComponent>()?.Posture == CharacterPosture.Sleep)
            {
                actor.GetComponent<NetworkStateComponent>().NetSession.Write("Not while you're sleeping.\n");
                return false;
            }

            if (!args.Any())
            {
                actor.GetComponent<NetworkStateComponent>().NetSession.Write("Say what, exactly?\n");
                return false;
            }

            var receiver = RoomSystem.FindRoom(actor).FindCharacter();
        }
    }

    public class CommandHandlerAttribute : Attribute
    {
        public string Command { get; }
        public string ArgumentList { get; }
        public string LongDescription { get; }
        public AccessLevel AccessLevel { get; }
        public bool CombatUsable { get; }
        public bool TypedCompletely { get; }

        public CommandHandlerAttribute(string command, string argumentList, string longDescription, AccessLevel accessLevel, bool combatUsable, bool typedCompletely)
        {
            Command = command;
            ArgumentList = argumentList;
            LongDescription = longDescription;
            AccessLevel = accessLevel;
            CombatUsable = combatUsable;
            TypedCompletely = typedCompletely;
            throw new NotImplementedException();
        }
    }

    public class CommandContainerAttribute : Attribute
    {
    }
}