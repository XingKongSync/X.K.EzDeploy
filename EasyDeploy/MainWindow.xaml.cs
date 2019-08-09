using EasyDeploy.Service;
using EasyDeploy.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyDeploy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<ServiceBaseInfo> serviceList = ServiceLocator.Instance.Value.GetServices();

            ServiceBaseInfo watchDogServiceInfo = null;

            ObservableCollection<ServiceVM> serviceVMCollection = new ObservableCollection<ServiceVM>();
            serviceList?.ForEach(s =>
            {
                if (WatchDogServiceVM.WATCHDOG_SERVICE_NAME.Equals(s.Config?.ServiceName))
                {
                    watchDogServiceInfo = s;
                }
                else
                {
                    serviceVMCollection.Add(new ServiceVM() { ServiceInfo = s });
                }
            });

            if (watchDogServiceInfo != null)
            {
                WatchDogServiceVM watchDogServiceVM = new WatchDogServiceVM() { ServiceInfo = watchDogServiceInfo };
                serviceVMCollection.Add(watchDogServiceVM);
                watchDogServiceVM.SetServiceVMList(serviceVMCollection);
            }

            icServiceContainer.ItemsSource = serviceVMCollection;
        }
    }
}
