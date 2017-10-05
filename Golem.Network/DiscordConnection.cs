using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Golem.Common.Interfaces;

namespace Golem.Network
{
    public class DiscordConnection
    {
        /// <summary>
        /// Use to attach event handlers
        /// </summary>
        private DiscordClient Client { get; }

        public event AsyncEventHandler<MessageCreateEventArgs> OnMessageReceive
        {
            add => Client.MessageCreated += value;
            remove => Client.MessageCreated -= value;
        }

        public DiscordConnection(string botToken, string botName)
        {
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = botToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            });            

            Client.MessageCreated += ClientOnMessageCreated;
        }

        private Task ClientOnMessageCreated(MessageCreateEventArgs args)
        {
            if (args.Message.Content.Equals("!login", StringComparison.InvariantCultureIgnoreCase))
            {
                Client.SendMessageAsync(args.Channel,
                    $"[Debug] Hi, {args.Author.Username}, login request acknowledge, assigning NetState");

                var netState = NetworkManager.CreateClient(Client, args.Author);
                netState.Start();
                Client.SendMessageAsync(args.Channel, $"[Debug] Created new NetState {netState.GetHashCode()}");
                netState.SendMessage("Hi, this should be a DM, <3 Golem");
            }

            return Task.FromResult(true);
        }

        public async void Start()
        {
            Console.WriteLine("Core::Network - Attempting connection");
            await Client.ConnectAsync();
        }
    }

    public static class NetworkManager
    {
        public static ICollection<INetState> NetStates { get; }

        static NetworkManager()
        {
            NetStates = new List<INetState>();
        }

        public static INetState CreateClient(DiscordClient client, DiscordUser user)
        {
            var netState = new NetState(client, user);

            if (NetStates.Contains(netState))
            {
                throw new NotSupportedException($"NetState for {user.Username} exists already");
            }

            NetStates.Add(netState);
            return netState;
        }

        public static void RemoveClient(INetState netState)
        {
            NetStates.Remove(netState);
        }
    }

    public class NetState : INetState, IDisposable
    {
        private long _lastActiveTick;
        private DiscordDmChannel _directMessage;

        public DiscordClient Client { get; }
        public DiscordUser User { get; set; }

        /// <inheritdoc />
        public IAccount Account { get; }

        /// <inheritdoc />
        public IMobile Mobile { get; }

        public NetState(DiscordClient client, DiscordUser user)
        {
            User = user;
            Client = client;
        }

        /// <inheritdoc />
        public async void Start()
        {
            _directMessage = Client.CreateDmAsync(User).Result;
        }

        /// <inheritdoc />
        public void SendMessage(string message)
        {
            _directMessage.SendMessageAsync(message);
        }

        /// <inheritdoc />
        public void CheckAlive(long curTicks)
        {
            if ((curTicks - _lastActiveTick) < TimeSpan.FromMinutes(5).Ticks)
            {
                Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            NetworkManager.RemoveClient(this);
        }
    }
}
