using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Golem.Server.Events;
using Golem.Server.Interfaces;

namespace Golem.Server.Network
{
    public class GlobalCommands
    {
        [Command("login")]
        public async Task LoginCommand(CommandContext e)
        {
            var client = e.Client;
            var netState = NetworkManager.CreateClient(client, e.Message.Author);
            netState.Start();

            EventSink.UserConnected?.Invoke(this, new UserConnected(netState));
        }

        [Command("userCount")]
        public async Task ConnectedUsersCommand(CommandContext e)
        {
            var client = e.Client;
            await client.SendMessageAsync(e.Channel, $"There are {NetworkManager.NetStates.Count} users currently connected");
        }
    }

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

            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                StringPrefix = "!"
            });

            commands.RegisterCommands<GlobalCommands>();
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
            Client.MessageCreated += async (args) => await Task.Run(() => OnClientOnMessageCreated(args));
        }

        private void OnClientOnMessageCreated(MessageCreateEventArgs args)
        {
            if (args.Channel.IsPrivate && args.Author == User)
            {
                EventSink.UserMessageReceived?.Invoke(this, new UserMessageReceived(this, args));
            }
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
