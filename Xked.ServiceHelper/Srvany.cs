using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Xked.ServiceHelper
{
    public class Srvany
    {
        private static readonly string INSTSRV_PATH;
        private static readonly string SRVANY_PATH;

        static Srvany()
        {
            INSTSRV_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\instsrv.exe");
            SRVANY_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\srvany.exe");
            if (!File.Exists(INSTSRV_PATH))
            {
                throw new FileNotFoundException("lib文件夹下的instsrv.exe不存在");
            }
            if (!File.Exists(SRVANY_PATH))
            {
                throw new FileNotFoundException("lib文件夹下的srvany.exe不存在");
            }
        }

        /// <summary>
        /// 将一个exe注册为服务
        /// </summary>
        /// <param name="srvName">服务名称</param>
        /// <param name="exePath">exe的启动路径</param>
        /// <param name="args">exe的启动参数</param>
        /// <returns></returns>
        public static bool InstallService(string srvName, string exePath, string args, string displayName = null, string desc = null)
        {
            if (IsServiceExist(srvName))
            {
                throw new InvalidOperationException($"服务（{srvName}）已存在，无法创建");
            }

            if (string.IsNullOrWhiteSpace(srvName))
            {
                throw new InvalidOperationException("服务名称不能为空");
            }

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"找不到可执行文件的路径\r\n{exePath}");
            }

            Process instsrvProc = new Process();
            instsrvProc.StartInfo = new ProcessStartInfo()
            {
                FileName = INSTSRV_PATH,
                Arguments = $"{srvName} \"{SRVANY_PATH}\""
            };
            instsrvProc.Start();
            instsrvProc.WaitForExit();

            AddRegistry(srvName, exePath, args, displayName, desc);

            return IsServiceExist(srvName);
        }

        /// <summary>
        /// 删除服务
        /// </summary>
        /// <param name="srvName">服务名称</param>
        public static void RemoveService(string srvName)
        {
            Process instsrvProc = new Process();
            instsrvProc.StartInfo = new ProcessStartInfo()
            {
                FileName = INSTSRV_PATH,
                Arguments = $"{srvName} REMOVE"
            };
            instsrvProc.Start();
            instsrvProc.WaitForExit();
            //RemoveRegistry(srvName);//删除服务时注册表项自动删除
        }

        /// <summary>
        /// 根据服务名称获取一个ServiceController
        /// </summary>
        /// <param name="srvName"></param>
        /// <returns></returns>
        public static ServiceController GetService(string srvName)
        {
            if (string.IsNullOrWhiteSpace(srvName))
                return null;

            var serviceControllers = ServiceController.GetServices();
            foreach (var srv in serviceControllers)
            {
                if (srv.ServiceName.Equals(srvName))
                {
                    return srv;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        /// <param name="srvName"></param>
        /// <returns></returns>
        public static bool IsServiceExist(string srvName)
        {
            return GetService(srvName) != null;
        }

        /// <summary>
        /// 判断服务是否正在运行
        /// </summary>
        /// <param name="srvName"></param>
        /// <returns></returns>
        public static bool IsServiceRunning(string srvName)
        {
            var service = GetService(srvName);
            return service?.Status == ServiceControllerStatus.Running;
        }

        /// <summary>
        /// 将服务要启动的实际exe路径写入注册表
        /// </summary>
        /// <param name="srvName">服务名称</param>
        /// <param name="exePath">要启动的实际exe路径</param>
        /// <param name="args">exe的启动参数</param>
        private static void AddRegistry(string srvName, string exePath, string args, string displayName = null, string desc = null)
        {
            string exeWorkDirectory = Path.GetDirectoryName(exePath);
            if (args == null)
            {
                args = string.Empty;
            }

            string srvRootKeyStr = $"SYSTEM\\CurrentControlSet\\Services\\{srvName}";
            RegistryKey srvRootKey = Registry.LocalMachine.OpenSubKey(srvRootKeyStr, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (srvRootKey != null)
            {
                displayName = displayName?.Trim();
                if (!string.IsNullOrWhiteSpace(displayName))
                {
                    srvRootKey.SetValue("DisplayName", displayName, RegistryValueKind.String);
                }
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    srvRootKey.SetValue("Description", desc, RegistryValueKind.String);
                }
            }

            string srvParamRegKeyStr = $"SYSTEM\\CurrentControlSet\\Services\\{srvName}\\Parameters";
            RegistryKey serviceRegKey = Registry.LocalMachine.OpenSubKey(srvParamRegKeyStr);
            if (serviceRegKey == null)
            {
                serviceRegKey = Registry.LocalMachine.CreateSubKey(srvParamRegKeyStr);
            }
            serviceRegKey.SetValue("Application", exePath, RegistryValueKind.String);
            serviceRegKey.SetValue("AppDirectory", exeWorkDirectory, RegistryValueKind.String);
            serviceRegKey.SetValue("AppParameters", args, RegistryValueKind.String);
            //与 SrvanyUI 兼容
            serviceRegKey.SetValue("SrvanyUI", "{637800A7-1458-425B-965D-EC8C0E750A72}", RegistryValueKind.String);

            serviceRegKey.Close();
        }

        //private static void RemoveRegistry(string srvName)
        //{
        //    RegistryKey serviceRegKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services");
        //    if (serviceRegKey != null)
        //    {
        //        serviceRegKey.DeleteSubKeyTree(srvName);
        //    }
        //}

        /// <summary>
        /// 运行服务
        /// </summary>
        /// <param name="srvName">服务名称</param>
        /// <returns>True：成功，False：失败</returns>
        public static bool RunService(string srvName)
        {
            var serviceControllers = ServiceController.GetServices();
            foreach (var srv in serviceControllers)
            {
                if (srv.ServiceName.Equals(srvName))
                {
                    try
                    {
                        srv.Start();
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="srvName">服务名称</param>
        /// <returns>True：成功，False：失败</returns>
        public static bool StopService(string srvName)
        {
            var serviceControllers = ServiceController.GetServices();
            foreach (var srv in serviceControllers)
            {
                if (srv.ServiceName.Equals(srvName))
                {
                    try
                    {
                        srv.Stop();
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return false;
        }
    }
}
