using System;
using Golem.Server.Enumerations;
using Golem.Server.Events;

namespace Golem.Server
{
    public class CommandDispatcher
    {
        public CommandDispatcher()
        {
            Console.WriteLine("Server:: CommandDispatcher initializing");

            EventSink.UserConnected += (sender, connected) => { connected.Ns.SendMessage("Hi"); };

            EventSink.UserMessageReceived += DispatchCommand;

            CommandSystem.Register("help", AccessLevel.Player, args => args.Mobile.SendMessage("Hi"));
        }

        private void DispatchCommand(object sender, UserMessageReceived message)
        {
            // TODO: Find the IMobile that belongs to the INetState and exchange, also implement IMobile :)
            CommandSystem.Handle(message.Ns, message.Args.Message.Content);
        }
    }
}