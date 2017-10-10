using System;
using System.Collections.Generic;
using Golem.Server.Enumerations;
using Golem.Server.Interfaces;

namespace Golem.Server
{
    public delegate void CommandEventHandler(CommandEventArgs e);

    public class CommandEventArgs : EventArgs
    {
        public IMobile Mobile { get; }

        public string Command { get; }

        public string ArgString { get; }

        public string[] Arguments { get; }

        public int Length => Arguments.Length;

        public string GetString(int index)
        {
            if (index < 0 || index >= Arguments.Length)
                return "";

            return Arguments[index];
        }

        public CommandEventArgs(IMobile mobile, string command, string argString, string[] arguments)
        {
            Mobile = mobile;
            Command = command;
            ArgString = argString;
            Arguments = arguments;
        }
    }

    public class CommandEntry : IComparable
    {
        public string Command { get; }

        public CommandEventHandler Handler { get; }

        public AccessLevel AccessLevel { get; }

        public CommandEntry(string command, CommandEventHandler handler, AccessLevel accessLevel)
        {
            Command = command;
            Handler = handler;
            AccessLevel = accessLevel;
        }

        public int CompareTo(object obj)
        {
            if (obj == this)
                return 0;
            else if (obj == null)
                return 1;

            CommandEntry e = obj as CommandEntry;

            if (e == null)
                throw new ArgumentException();

            return String.Compare(Command, e.Command, StringComparison.Ordinal);
        }
    }

    public static class CommandSystem
    {
        private static Dictionary<string, CommandEntry> Entries { get; }

        static CommandSystem()
        {
            Entries = new Dictionary<string, CommandEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public static void Register(string command, AccessLevel access, CommandEventHandler handler)
        {
            Entries[command] = new CommandEntry(command, handler, access);
        }

        public static AccessLevel BadCommandIgnoreLevel { get; set; } = AccessLevel.Player;

        public static bool Handle(IMobile from, string text)
        {
            int indexOf = text.IndexOf(' ');

            string command;
            string[] args;
            string argString;

            if (indexOf >= 0)
            {
                argString = text.Substring(indexOf + 1);

                command = text.Substring(0, indexOf);
                args = Split(argString);
            }
            else
            {
                argString = "";
                command = text.ToLower();
                args = new string[0];
            }

            CommandEntry entry = null;
            Entries.TryGetValue(command, out entry);

            if (entry != null)
            {
                if (from.AccessLevel >= entry.AccessLevel)
                {
                    if (entry.Handler != null)
                    {
                        CommandEventArgs e = new CommandEventArgs(from, command, argString, args);
                        entry.Handler(e);
                    }
                }
                else
                {
                    if (from.AccessLevel <= BadCommandIgnoreLevel)
                        return false;

                    from.SendMessage("You do not have access to that command.");
                }
            }
            else
            {
                if (from.AccessLevel <= BadCommandIgnoreLevel)
                    return false;

                from.SendMessage("That is not a valid command.");
            }

            return true;
        }

        public static string[] Split(string value)
        {
            var array = value.ToCharArray();
            var list = new List<string>();

            int start = 0, end = 0;

            while (start < array.Length)
            {
                var c = array[start];

                if (c == '"')
                {
                    ++start;
                    end = start;

                    while (end < array.Length)
                        if (array[end] != '"' || array[end - 1] == '\\')
                            ++end;
                        else
                            break;

                    list.Add(value.Substring(start, end - start));

                    start = end + 2;
                }
                else if (c != ' ')
                {
                    end = start;

                    while (end < array.Length)
                        if (array[end] != ' ')
                            ++end;
                        else
                            break;

                    list.Add(value.Substring(start, end - start));

                    start = end + 1;
                }
                else
                {
                    ++start;
                }
            }

            return list.ToArray();
        }
    }
}