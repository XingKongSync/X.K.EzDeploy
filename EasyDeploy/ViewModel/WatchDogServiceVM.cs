using EasyDeploy.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchDogService.Config;
using Xked.ServiceHelper;

namespace EasyDeploy.ViewModel
{
    class WatchDogServiceVM : ServiceVM
    {
        public static readonly string WATCHDOG_SERVICE_NAME = "WatchDogService";

        private List<ServiceVM> _serviceList = null;

        private ServiceBaseInfo selfInfo = null;
        private string selfFullExcutableFilePath = null;

        public override void UpdateStatus()
        {
            if (!string.IsNullOrWhiteSpace(selfFullExcutableFilePath))
            {
                if (Srvany.IsServiceRunning(WATCHDOG_SERVICE_NAME))
                {
                    if (!Srvany.IsProcessExists(selfFullExcutableFilePath))
                    {
                        Srvany.StopService(WATCHDOG_SERVICE_NAME);
                    }
                }
            }
            base.UpdateStatus();
        }

        public void SetServiceVMList(IEnumerable<ServiceVM> serviceVMList)
        {
            _serviceList = new List<ServiceVM>();
            if (serviceVMList != null)
            {
                foreach (var srvVM in serviceVMList)
                {
                    if (srvVM.ServiceInfo != null)
                    {
                        if (WATCHDOG_SERVICE_NAME.Equals(srvVM.ServiceInfo.Config?.ServiceName))
                        {
                            selfInfo = srvVM.ServiceInfo;

                            string serviceBaseDir = selfInfo?.BaseDirectory;
                            if (!string.IsNullOrEmpty(serviceBaseDir))
                            {
                                string watchDogExeFullPath = Path.Combine(serviceBaseDir, selfInfo?.Config?.ExcutableFilePath);
                                if (File.Exists(watchDogExeFullPath))
                                {
                                    selfFullExcutableFilePath = watchDogExeFullPath;
                                }
                            }
                        }
                        else
                        {
                            _serviceList.Add(srvVM);
                        }
                    }
                }
            }
        }

        protected override void InstallCommandHandler()
        {
            SaveConfig();
            base.InstallCommandHandler();
        }

        /// <summary>
        /// 将新的配置写入到配置文件中
        /// </summary>
        private void SaveConfig()
        {
            string serviceBaseDir = selfInfo?.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(serviceBaseDir))
            {
                string configFilePath = Path.Combine(serviceBaseDir, @"Config.json");
                string configJsonStr = string.Empty;

                ConfigEnitty config = new ConfigEnitty();
                config.ServiceList = new List<ServiceWatchInfo>();
                if (_serviceList != null)
                {
                    foreach (var srvVM in _serviceList)
                    {
                        if (!srvVM.Installed)
                            continue;

                        ServiceBaseInfo srvInfo = srvVM.ServiceInfo;
                        if (!CheckServiceInfoValid(srvInfo))
                            continue;

                        config.ServiceList.Add(new ServiceWatchInfo()
                        {
                            AutoRecovery = true,
                            FullExePath = Path.Combine(srvInfo.BaseDirectory, srvInfo.Config?.ExcutableFilePath),
                            ServiceName = srvInfo.Config?.ServiceName
                        });
                    }
                    configJsonStr = JsonConvert.SerializeObject(config, Formatting.Indented);
                }

                File.WriteAllText(configFilePath, configJsonStr);
            }
        }

        private bool CheckServiceInfoValid(ServiceBaseInfo baseInfo)
        {
            if (baseInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(baseInfo.BaseDirectory))
                {
                    if (baseInfo.Config != null)
                    {
                        if (!string.IsNullOrWhiteSpace(baseInfo.Config.ExcutableFilePath))
                        {
                            if (!string.IsNullOrWhiteSpace(baseInfo.Config.ServiceName))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
