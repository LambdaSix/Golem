using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp;
using DiscordSharp.Objects;

namespace Golem.Network
{
    public class DiscordConnection
    {
        /// <summary>
        /// Use to attach event handlers
        /// </summary>
        public DiscordClient Client { get; }
        public string BotName { get; set; }

        public DiscordConnection(string botToken, string botName)
        {
            Client = new DiscordClient(botToken, true);
            BotName = botName;
        }

        public void Start()
        {
            Client.Connected +=
                (s, a) =>
                {
                    Console.WriteLine($"Core::Network - Connected to Discord as {a.User.Username}");
                    Client.ChangeClientInformation(new DiscordUserInformation()
                    {
                        Username = BotName,
                    });
                };

            Console.WriteLine("Core::Network - Attempting connection");
            Client.SendLoginRequest();
            Client.Connect();
        }
    }
}
