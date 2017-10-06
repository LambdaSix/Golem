using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.EventArgs;
using Golem.Common.Events;
using Golem.Common.Interfaces;
using Golem.Network;

namespace Golem.Game
{
    public class CommandDispatcher
    {
        // TODO: Rewrap MessageCreateEventArgs to something less Discord-Specific
        private Dictionary<string,Action<INetState, MessageCreateEventArgs>> CommandMap { get; set; }

        public CommandDispatcher()
        {
            Console.WriteLine("Server:: CommandDispatcher initializing");

            EventSink.UserConnected += (sender, connected) =>
            {
                connected.Ns.SendMessage("Hi");
            };

            EventSink.UserMessageReceived += DispatchCommand;
        }

        public void RegisterCommand(string commandName, Action<INetState, MessageCreateEventArgs> command)
        {
            if (CommandMap.ContainsKey(commandName))
                throw new NotSupportedException($"Command '{commandName}' already registered");

            CommandMap.Add(commandName, command);
        }

        public void UnregisterCommand(string commandName)
        {
            if (CommandMap.ContainsKey(commandName))
                CommandMap.Remove(commandName);
            else
                throw new NotSupportedException($"Command '{commandName}' has not been registered");
        }

        private void DispatchCommand(object sender, UserMessageReceived message)
        {
            Action<INetState, MessageCreateEventArgs> maybeCommand = null;
            var command = message.Args.Message.Content.Split(' ').First();

            if (CommandMap.TryGetValue(command, out maybeCommand))
            {
                maybeCommand?.Invoke(message.Ns, message.Args);
            }
        }
    }
}
