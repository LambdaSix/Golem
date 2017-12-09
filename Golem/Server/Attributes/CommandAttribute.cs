using System;
using Golem.Server.Enumerations;

namespace Golem.Server.Attributes
{
    public class CommandAttribute : Attribute
    {
        private readonly string _command;
        public AccessLevel AccessLevel { get; }

        public CommandAttribute(string command, AccessLevel accessLevel)
        {
            _command = command;
            AccessLevel = accessLevel;
        }
    }
}