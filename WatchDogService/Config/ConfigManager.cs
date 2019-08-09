using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchDogService.Config
{
    class ConfigManager
    {
        public static Lazy<ConfigManager> Instance = new Lazy<ConfigManager>(() => new ConfigManager());

        private readonly string AppBasePath;

        public ConfigEnitty Config;

        private ConfigManager()
        {
            AppBasePath = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                string configFilePath = Path.Combine(AppBasePath, @"Config.json");
                if (File.Exists(configFilePath))
                {
                    string configJson = File.ReadAllText(configFilePath);
                    Config = JsonConvert.DeserializeObject<ConfigEnitty>(configJson);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
