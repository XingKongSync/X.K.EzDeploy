using EasyDeploy.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xked.ServiceHelper;

namespace EasyDeploy.ViewModel
{
    class ServiceVM : INotifyPropertyChanged
    {
        private static DispatcherTimer _timer;
        static ServiceVM()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string callerMemberName = null)
        {
            if (!string.IsNullOrWhiteSpace(callerMemberName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerMemberName));
        }

        private string _displayName;
        private string _serviceName;
        private string _description;
        private bool _installed = false;
        private bool _isRunning = false;
        private ServiceBaseInfo _serviceInfo;

        public bool Installed
        {
            get => _installed;
            set
            {
                _installed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRun));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRun));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        public bool CanRun
        {
            get
            {
                return Installed && !IsRunning;
            }
        }

        public bool CanStop
        {
            get
            {
                return Installed && IsRunning;
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public string ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        internal ServiceBaseInfo ServiceInfo
        {
            get => _serviceInfo;
            set
            {
                _serviceInfo = value;
                UpdateStatus();
            }
        }

        public ICommand InstallCommand { get; private set; }
        public ICommand UninstallCommand { get; private set; }
        public ICommand StartServiceCommand { get; private set; }
        public ICommand StopServiceCommand { get; private set; }
        public ICommand OpenFolderCommand { get; private set; }

        public ServiceVM()
        {
            InstallCommand = new DelegateCommand(InstallCommandHandler);
            UninstallCommand = new DelegateCommand(UninstallCommandHandler);
            StartServiceCommand = new DelegateCommand(StartServiceCommandHandlerAsync);
            StopServiceCommand = new DelegateCommand(StopServiceCommandHandler);
            OpenFolderCommand = new DelegateCommand(OpenFolderCommandHandler);

            _timer.Tick -= Timer_Tick;
            _timer.Tick += Timer_Tick;
        }

        ~ServiceVM()
        {
            _timer.Tick -= Timer_Tick;
        }

        /// <summary>
        /// 打开服务所在的文件夹
        /// </summary>
        private void OpenFolderCommandHandler()
        {
            if (ServiceInfo != null && !string.IsNullOrWhiteSpace(ServiceInfo.BaseDirectory))
            {
                Process.Start(ServiceInfo.BaseDirectory);
            }
        }

        /// <summary>
        /// 更新服务状态
        /// </summary>
        /// <param name="serviceInfo"></param>
        public void UpdateStatus()
        {
            if (ServiceInfo != null && ServiceInfo.Config != null)
            {
                var config = ServiceInfo.Config;

                string serviceName = config.ServiceName;
                Installed = Srvany.IsServiceExist(serviceName);
                IsRunning = Srvany.IsServiceRunning(serviceName);

                DisplayName = config.DisplayName;
                ServiceName = config.ServiceName;
                Description = config.Description;
            }
            else
            {
                Installed = false;
                IsRunning = false;
                Description = ServiceName = DisplayName = string.Empty;
            }
        }

        protected virtual void InstallCommandHandler()
        {
            try
            {
                if (ServiceInfo != null && ServiceInfo.Config != null)
                {
                    var config = ServiceInfo.Config;

                    //必须使用Exe的完整路径
                    string fullExePath = Path.Combine(ServiceInfo.BaseDirectory, config.ExcutableFilePath);
                    //安装服务
                    Srvany.InstallService(config.ServiceName, fullExePath, config.Args, config.DisplayName, config.Description);

                    //刷新界面
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void UninstallCommandHandler()
        {
            try
            {
                if (ServiceInfo != null && ServiceInfo.Config != null)
                {
                    var config = ServiceInfo.Config;

                    //卸载前先停止服务
                    try { Srvany.StopService(config.ServiceName); } catch (Exception) { }
                    //卸载服务
                    Srvany.RemoveService(config.ServiceName);

                    //刷新界面
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void StartServiceCommandHandlerAsync()
        {
            try
            {
                if (ServiceInfo != null && ServiceInfo.Config != null)
                {
                    var config = ServiceInfo.Config;

                    Srvany.RunService(config.ServiceName);

                    //刷新界面
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        private void StopServiceCommandHandler()
        {
            try
            {
                if (ServiceInfo != null && ServiceInfo.Config != null)
                {
                    var config = ServiceInfo.Config;

                    Srvany.StopService(config.ServiceName);

                    //刷新界面
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
        }

        /// <summary>
        /// 定时更新界面状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }
    }
}
