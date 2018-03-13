using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using Capsicum;
using Golem.Game;
using Golem.Server.Database;
using Golem.Server.Network;
using Golem.Server.Session;
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

    public enum LogType
    {
        Info,
        Warning,
        Error,
    }

    /// <summary>
    /// artificial delay for commands
    /// </summary>
    public enum TickDelay
    {
        Instant = 1,
        Single = 650,
        Double = Single * 2,
        Triple = Single * 3,
        Quadruple = Single * 4,
        Quintuple = Single * 5,
        Sextuple = Single * 6,
        Septuple = Single * 7,
        Octuple = Single * 8,
        Nonuple = Single * 9,
        Decuple = Single * 10,
        Reroll = Single * 20,
    }

    public enum Pools
    {
        General = 0,
        Mobiles = 1,
        Items = 2,

    }

    public class ServerConstants
    {
        public static string WelcomeRoom = "Welcome Room";
        public static object StartRoom = "The Academy";

        public static bool AutoApprovedEnabled = true;
        public static int DeadHitpoints = -10;
        public static int IncapacitatedHitpoints = -3;
        public static double CorpseDecayTimeMs = TimeSpan.FromMinutes(30).TotalMilliseconds;
        public static double MobWalkInterval = TimeSpan.FromMinutes(5).TotalMilliseconds;
        public static double RegenTime = TimeSpan.FromSeconds(30).TotalMilliseconds;
        public static double AgeTime = TimeSpan.FromMinutes(1).TotalMilliseconds;

        public static double TickRate = TimeSpan.FromMilliseconds(20).TotalMilliseconds;
        public static double TickTime = 1000 / TickRate;
        public static double CombatTickRate = TimeSpan.FromSeconds(1).TotalMilliseconds;
    }

    public class GolemServer : IDisposable
    {
        public IConnectionListener ConnListener { get; }
        public IConnectionMonitor ConnMonitor { get; }
        public ISessionMonitor SessionMonitor { get; }
        public IDatabase Database { get; }
//        public IEnumerable<IHandler> Handlers { get; }

        public Dictionary<Pools, Pool> Pools { get; } = new Dictionary<Pools, Pool>();

        public IEnumerable<CombatSkill> CombatSkills { get; set; }

        public static GolemServer Current { get; private set; }
        public Random Random { get; } = new Random();

        private bool _closing;
        private readonly AutoResetEvent _signal = new AutoResetEvent(true);
        private bool IsDebug { get; set; }
        
        // TODO: Make a singleton WorldState?
        public DateTime CurrentTime { get; set; }

        public void Set() => _signal.Set();

        public GolemServer(
            IConnectionListener connListener,
            IConnectionMonitor connMonitor,
            ISessionMonitor sessionMonitor,
            IDatabase database
            //IDynamicCommandLookup dynamicCommandLookup,
            //IEnumerable<IHandler> handlers
            )
        {
            ConnListener = connListener;
            ConnMonitor = connMonitor;
            SessionMonitor = sessionMonitor;
            Database = database;
            //Handlers = handlers;
            Current = this;

            Pools = new Dictionary<Pools, Pool>()
            {
                [Server.Pools.General] = new Pool(),
                [Server.Pools.Mobiles] = new Pool(),
                [Server.Pools.Items] = new Pool(),
            };
        }

        public void Main(ServerOptions options)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            Console.WriteLine($"Golem - Version {version}");
            Console.WriteLine($"Core: Running on .net framework {Environment.Version}");

            SetupEnvironment();
            SetupNetwork();

            CreateKernel();

            // TODO: Rate-limit the server processing.
            while (!_closing)
            {
                _signal.WaitOne();

                // Process game loop

                var nextTickDue = false;
                TimeSpan span;

                while (!nextTickDue)
                {
                    var elapsedTime = DateTime.UtcNow;
                    span = elapsedTime - CurrentTime;

                    if (span.TotalMilliseconds > 1000 / TimeConstants.TicksPerSecond)
                        nextTickDue = false;
                    else
                    {
                        int sleepTime = (int) ((1000 / TimeConstants.TicksPerSecond) - span.TotalMilliseconds);
                        Thread.Sleep(sleepTime);
                        nextTickDue = true;
                    }
                }

                CurrentTime = DateTime.UtcNow;
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
            
        }

        public IKernel CreateKernel()
        {
            Console.WriteLine("Core: Searching & Registering Services/Modules");
            var kernel = new StandardKernel();
            //NinjectBootstrap.LoadModules(kernel);

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

        public void Log(LogType logType, string message)
        {
            try
            {
                var log = $"{logType}: {message}";
                File.AppendAllText("Log\\server.log", log);
            }
            catch
            {

            }
        }

        public void Log(string message, bool newLine = true)
        {
            Console.Write($"{DateTime.Now,-10:mm:ss.fff}: {message}{(newLine ? "\n" : string.Empty)}");
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
