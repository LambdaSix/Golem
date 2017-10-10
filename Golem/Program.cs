using System;
using System.Linq.Expressions;
using Golem.Server;
using Golem.Server.OptionsParse;

namespace Golem
{
    public class ArgumentOptions
    {
        [HelpText("Start the server in Debug mode")]
        public bool Debug { get; set; }

        [HelpText("Show this help message and exit")]
        public bool ShowHelp { get; set; }
    }

    class Program
    {
        private const int EXIT_SUCCESS = 0;
        private const int EXIT_FAILURE = 1;
        private const string AssemblyName = "Golem";

        private static void Die(string message)
        {
            Console.WriteLine("{0}: ", AssemblyName);
            Console.WriteLine(message);
            Console.WriteLine("Try '{0} --help'", AssemblyName);
            Environment.Exit(EXIT_FAILURE);
        }

        static void Main(string[] args)
        {
            var config = new ArgumentOptions();
            var optionSet = new OptionSet {
                {"d|debug", GetHelpText(() => config.Debug), v => config.Debug = v != null},
                {"h|help", GetHelpText(() => config.ShowHelp), v => config.ShowHelp = v != null}
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException ex)
            {
                Die(ex.Message);
            }

            if (config.ShowHelp)
                ShowHelpMessage(optionSet);

            using (var server = new GolemServer())
            {
                server.Main(TranslateConfig(config));
            }
        }

        private static ServerOptions TranslateConfig(ArgumentOptions config)
        {
            return new ServerOptions()
            {
                Debug = config.Debug
            };
        }

        private static void ShowHelpMessage(OptionSet set)
        {
            Console.WriteLine("Usage: {0} [OPTION+] ", AssemblyName);
            Console.WriteLine("Start an instance of the Golem server");
            Console.WriteLine();
            Console.WriteLine("Options: ");
            set.WriteOptionDescriptions(Console.Out);
            Environment.Exit(1);
        }

        private static string GetHelpText<T>(Expression<Func<T>> propertyExpr)
        {
            return OptionSetExtras.GetHelpText(propertyExpr);
        }
    }
}
