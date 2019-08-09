using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyDeploy.Service
{
    class ServiceLocator
    {
        public static Lazy<ServiceLocator> Instance = new Lazy<ServiceLocator>(() => new ServiceLocator());

        private readonly string ConfigFileName = "ServiceConfig.json";
        private readonly string ServicePath;

        private ServiceLocator()
        {
            ServicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Service");
            //防止文件夹不存在
            Directory.CreateDirectory(ServicePath);
        }

        public List<ServiceBaseInfo> GetServices()
        {
            List<ServiceBaseInfo> result = new List<ServiceBaseInfo>();
            var dirs = Directory.EnumerateDirectories(ServicePath);
            foreach (var dir in dirs)
            {
                string configFilePath = Path.Combine(ServicePath, dir, ConfigFileName);
                if (File.Exists(configFilePath))
                {
                    try
                    {
                        string configStr = File.ReadAllText(configFilePath);
                        ServiceConfigEntity config = JsonConvert.DeserializeObject<ServiceConfigEntity>(configStr);

                        result.Add(new ServiceBaseInfo() { BaseDirectory = Path.Combine(ServicePath, dir), Config = config });
                    }
                    catch (Exception ex)
                    {
                        //解析ServiceConfig.json出错
                    }
                }
            }
            return result;
        }
    }
}
