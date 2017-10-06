using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Golem.Common;
using Golem.Common.Events;

namespace Golem.Game
{
    public class ModuleInitializer : IModuleInitializer
    {
        /// <inheritdoc />
        public void Load()
        {
            Console.WriteLine("Registering Game Module!");
            var commandDispatcher = new CommandDispatcher();
        }
    }

    public class CommandDispatcher
    {
        static CommandDispatcher()
        {
            EventSink.UserConnected += (sender, connected) =>
            {
                connected.Ns.SendMessage("Hi");
            };

            EventSink.UserMessageReceived += DispatchCommand;
        }

        private static void DispatchCommand(object sender, UserMessageReceived userMessageReceived)
        {
            if (userMessageReceived.Args.Message.Content == "hi")
            {
                userMessageReceived.Ns.SendMessage("Hello!");
            }
        }
    }
}
