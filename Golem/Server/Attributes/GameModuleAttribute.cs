using System;

namespace Golem.Server.Attributes
{
    public class GameModuleAttribute : Attribute
    {
        public GameModuleAttribute()
        {
            
        }
    }

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