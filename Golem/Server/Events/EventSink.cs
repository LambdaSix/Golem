using System;
using DSharpPlus.EventArgs;
using Golem.Server.Interfaces;

namespace Golem.Server.Events
{
    public static class EventSink
    {
        public static EventHandler<UserMessageReceived> UserMessageReceived { get; set; }
        public static EventHandler<UserConnected> UserConnected { get; set; }
    }

    public class UserMessageReceived : EventArgs
    {
        public INetState Ns { get; }
        public MessageCreateEventArgs Args { get; }

        public UserMessageReceived(INetState ns, MessageCreateEventArgs args)
        {
            Ns = ns;
            Args = args;
        }
    }

    public class UserConnected : EventArgs
    {
        public INetState Ns { get; }

        public UserConnected(INetState ns)
        {
            Ns = ns;
        }
    }
}