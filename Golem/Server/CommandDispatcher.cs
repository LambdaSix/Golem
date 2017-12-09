using System;
using System.Linq;
using Golem.Database;
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

            var db = new GolemDbContext();
            var a = db.Components.Where(s => s.Id > 0).ToList();

            foreach (var item in a)
            {
                Console.WriteLine($"{item.Id},{item.ComponentTypeId},{item.ComponentData}");
            }
        }

        private void DispatchCommand(object sender, UserMessageReceived message)
        {
            // TODO: Find the IMobile that belongs to the INetState and exchange, also implement IMobile :)
            //CommandSystem.Handle(message.Ns, message.Args.Message.Content);
        }
    }
}