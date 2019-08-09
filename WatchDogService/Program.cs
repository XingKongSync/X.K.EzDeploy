using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatchDogService.Config;
using Xked.ServiceHelper;

namespace WatchDogService
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigManager.Instance.Value.Config;

            IEnumerable<ServiceWatchInfo> serviceInfos = config?.ServiceList;
            if (serviceInfos != null)
            {
                while (true)
                {
                    try
                    {
                        foreach (var srvInfo in serviceInfos)
                        {
                            if (!IsServiceInfoValid(srvInfo))
                                continue;

                            //先负责纠正服务状态
                            //因为有可能服务显示“正在运行”但是进程列表里没有那个进程
                            if (IsServiceRunning(srvInfo.ServiceName))
                            {
                                if (!IsProcessExists(srvInfo.FullExePath))
                                {
                                    Srvany.StopService(srvInfo.ServiceName);
                                }
                            }
                            else
                            {
                                //如果需要守护该服务，则发现服务没在运行后，把该服务启动
                                Srvany.RunService(srvInfo.ServiceName);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    Thread.Sleep(2000);
                }
            }
        }

        static bool IsServiceInfoValid(ServiceWatchInfo info)
        {
            if (info != null)
            {
                if (!string.IsNullOrWhiteSpace(info.ServiceName))
                {
                    if (!string.IsNullOrWhiteSpace(info.FullExePath))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool IsServiceRunning(string srvName)
        {
            return Srvany.IsServiceRunning(srvName);
        }

        static bool IsProcessExists(string fullExePath)
        {
            string exeFileName = Path.GetFileNameWithoutExtension(fullExePath);
            var procs = Process.GetProcessesByName(exeFileName);
            if (procs != null)
            {
                foreach (var proc in procs)
                {
                    if (proc.MainModule.FileName.Equals(fullExePath))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
