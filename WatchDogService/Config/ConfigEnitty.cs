using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchDogService.Config
{
    public class ConfigEnitty
    {
        public List<ServiceWatchInfo> ServiceList;
    }

    public class ServiceWatchInfo
    {
        public string FullExePath;
        public string ServiceName;
        public bool AutoRecovery;
    }
}
