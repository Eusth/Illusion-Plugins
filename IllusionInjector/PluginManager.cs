using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace IllusionInjector
{
    static class PluginManager
    {

        public static IEnumerable<IPlugin> LoadPlugins()
        {
            string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");


            if (!Directory.Exists(pluginDirectory)) return new IPlugin[0];

            String[] files = Directory.GetFiles(pluginDirectory, "*.dll");
            List<IPlugin> plugins = new List<IPlugin>();
            foreach (var s in files)
            {
                plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s)));
            }
            

            // DEBUG
            Console.WriteLine("Loading plugins from {0} and found {1}", pluginDirectory, plugins.Count);
            Console.WriteLine("-----------------------------");
            foreach (var plugin in plugins)
            {

                Console.WriteLine(" {0}: {1}", plugin.Name, plugin.Version);
            }
            // ---

            return plugins;
        }

        private static IEnumerable<IPlugin> LoadPluginsFromFile(string file)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return plugins;

            try
            {
                Assembly assembly = Assembly.LoadFile(file);

                foreach (Type t in assembly.GetTypes())
                {
                    if (t.GetInterface("IPlugin") != null)
                    {
                        try
                        {
                            IPlugin pluginInstance = Activator.CreateInstance(t) as IPlugin;

                            plugins.Add(pluginInstance);
                            //return;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

            }
            catch (Exception)
            {
            }

            return plugins;
        }

    }
}
