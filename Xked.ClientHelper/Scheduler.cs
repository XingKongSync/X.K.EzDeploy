using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskScheduler;

namespace Xked.ClientHelper
{
    public class Scheduler
    {
        private static TaskSchedulerClass scheduler = new TaskSchedulerClass();

        private static string GetUserId()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        /// <summary>
        /// 如果任务调度器未连接，则连接
        /// </summary>
        private static void TryConnectScheduler()
        {
            lock (scheduler)
            {
                if (!scheduler.Connected)
                {
                    scheduler.Connect(
                     "",//电脑名或者IP
                     "",//用户名
                     "",//域名
                     ""//密码
                   );
                }
            }
        }

        public static bool TryGetTask(string taskName, out IRegisteredTask outTask)
        {
            outTask = null;
            TryConnectScheduler();
            ITaskFolder folder = scheduler.GetFolder("\\");

            IRegisteredTaskCollection tasks = folder.GetTasks(1);

            foreach (IRegisteredTask task in tasks)
            {
                if (task.Name.Equals(taskName))
                {
                    outTask = task;
                    break;
                }
            }

            return outTask != null;
        }

        public static IRegisteredTask InstallAutoRunClient(string taskName, string exeFullPath, string taskDesc)
        {
            TryConnectScheduler();

            if (TryGetTask(taskName, out var outtask))
            {
                throw new InvalidOperationException("计划任务已存在");
            }

            Uri uri = new Uri(exeFullPath);
            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("必须指定应用程序的完整路径");
            }

            ITaskFolder folder = scheduler.GetFolder("\\");

            //获取UserId
            string userId = GetUserId();

            //创建计划任务
            ITaskDefinition myTask = scheduler.NewTask(0);
            myTask.RegistrationInfo.Author = userId;//任务创建者的名字
            myTask.RegistrationInfo.Description = taskDesc;//任务描述

            myTask.Principal.UserId = userId;
            myTask.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;//使用最高权限运行

            myTask.Settings.RunOnlyIfIdle = false;

            //创建登陆触发器
            ILogonTrigger trigger = (ILogonTrigger)myTask.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
            trigger.UserId = userId;//指定登陆用户
            trigger.Enabled = true;

            //其他设置
            myTask.Settings.ExecutionTimeLimit = "PT0S";//任务运行超时时长：无限
            myTask.Settings.MultipleInstances = _TASK_INSTANCES_POLICY.TASK_INSTANCES_IGNORE_NEW;//不允许多实例
            myTask.Settings.DisallowStartIfOnBatteries = false;//当使用电池时仍然执行
            myTask.Settings.AllowHardTerminate = true;//允许强行终止任务
            myTask.Settings.RunOnlyIfNetworkAvailable = false;//取消仅当网络可用时运行
            myTask.Settings.AllowDemandStart = true;//允许按需执行
            myTask.Settings.Enabled = true;
            myTask.Settings.WakeToRun = false;//不需要唤醒计算机时运行此任务

            //设置操作
            IExecAction action = (IExecAction)myTask.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
            action.Path = exeFullPath;//要启动的程序的路径

            Console.WriteLine(myTask.XmlText);

            //注册任务
            IRegisteredTask regTask = folder.RegisterTaskDefinition(
                taskName,
                myTask,
                (int)_TASK_CREATION.TASK_CREATE,
                null, //user
                null, //password
                _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                ""
            );
            return regTask;
        }
    }
}
