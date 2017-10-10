using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using Golem.Server.ModuleLoader;
using Golem.Server.Network;
using Ninject;

namespace Golem.Server
{
    /// <summary>
    /// PArsed and clean variant of ArgumentOptions
    /// </summary>
    public class ServerOptions
    {
        public bool Debug { get; set; }
    }

    public class GolemServer : IDisposable
    {
        private bool _closing;
        private readonly AutoResetEvent _signal = new AutoResetEvent(true);
        private bool IsDebug { get; set; }

        private DiscordConnection Network { get; set; }
        private CommandDispatcher CommandDispatcher { get; set; }

        public void Set() => _signal.Set();

        public void Main(ServerOptions options)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            Console.WriteLine($"Golem - Version {version}");
            Console.WriteLine($"Core: Running on .net framework {Environment.Version}");

            SetupEnvironment();
            SetupNetwork();

            CreateKernel();
            Network.Start();
            // TODO: Issue global messages for ServerStarted
            CommandDispatcher = new CommandDispatcher();

            while (!_closing)
            {
                _signal.WaitOne();
                // TODO: Slice game tick
                // TODO: Slice message pump
                // TODO: Process any disposed network handles
            }
        }

        public void SetupEnvironment()
        {
            if (MachineInfo.IsMultiProcessor && MachineInfo.Is64Bit)
                Console.WriteLine("Core: Configuring for {0} {1}processor{2}",
                    MachineInfo.ProcessorCount,
                    MachineInfo.Is64Bit ? "64-bit " : "",
                    MachineInfo.ProcessorCount == 1 ? "" : "s");

            if (MachineInfo.IsUnixHost)
                Console.WriteLine("Core: Unix environment detected");

            if (MachineInfo.IsMacOsxHost)
                Console.WriteLine("Core: MacOSX environment detected");

            if (GCSettings.IsServerGC)
                Console.WriteLine("Core: Server garbage collection mode enabled");
        }

        public void SetupNetwork()
        {
            Network = new DiscordConnection(Secrets.RetrieveBotKey(), "Golem");
        }

        public IKernel CreateKernel()
        {
            Console.WriteLine("Core: Searching & Registering Services/Modules");
            var kernel = new StandardKernel();
            NinjectBootstrap.LoadModules(kernel);

            return kernel;
        }

        /// <inheritdoc />
        public void Dispose() => HandleClosed();

        public void HandleClosed()
        {
            if (_closing) return;

            _closing = true;
            Console.WriteLine("Core: Shutting down");
        }
    }

    public static class MachineInfo
    {
        public static int ProcessorCount => Environment.ProcessorCount;

        public static bool IsMultiProcessor => ProcessorCount > 1;

        public static bool Is64Bit => Environment.Is64BitProcess;

        public static bool Is32Bit => !Is64Bit;

        public static PlatformID Platform => Environment.OSVersion.Platform;

        // 128 == Mono
        public static bool IsUnixHost => (Platform == PlatformID.Unix || (int)Platform == 128);

        public static bool IsMacOsxHost => (Platform == PlatformID.MacOSX);
    }

    public static class Secrets
    {
        public static string RetrieveBotKey()
        {
            if (!File.Exists("botkey.txt"))
            {
                throw new FileNotFoundException("Unable to find 'botkey.txt' to retrieve secret key from");
            }

            var key = File.ReadAllLines("botkey.txt");
            return key.First();
        }
    }
}
