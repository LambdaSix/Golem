using System;

namespace Golem.Server.Attributes
{
    public class GameCommand : Attribute
    {
        public string Command { get; }
        public string HelpText { get; }

        public GameCommand(string command, string helpText = null)
        {
            Command = command;
            HelpText = helpText;
        }
    }
}