using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ninject;

namespace Golem.Server.ModuleLoader
{
    public static class NinjectBootstrap
    {
        public static void LoadModules(IKernel kernel, string searchPath = "./")
        {
            foreach (var file in Directory.EnumerateFiles(searchPath, "*.dll"))
            {
                var assembly = Assembly.LoadFrom(file);

                if (assembly.GetTypes().Any(t => t.GetInterfaces().Contains(typeof(IModuleInitializer))))
                {
                    Console.WriteLine($"Core: Discovered module - '{assembly.GetName().Name}'");
                    kernel.Load(file);
                }
            }
        }

        public static IEnumerable<IModuleInitializer> FindAllModules(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .Select(assembly => assembly.GetTypes()
                    .Where(s => s.GetInterfaces().Any(i => i.Name == typeof(IModuleInitializer).Name)))
                .SelectMany(modules => modules.Select(Activator.CreateInstance).OfType<IModuleInitializer>());
        }
    }

    public interface IModuleInitializer
    {
        /// <summary>
        /// Bind relationships this module is responsible for.
        /// </summary>
        void Load();
    }
}
