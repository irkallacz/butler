using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
using System.ServiceProcess;

namespace Butler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServiceController[] services;

        public MainWindow()
        {
            InitializeComponent();
            services = ServiceController.GetServices(Environment.MachineName);
            loadConfig();
        }

        private void loadConfig()
        {
            var xml = XDocument.Load("services.xml");
            var items = from item in xml.Descendants("service") select item;
            int i = 0;

            foreach (var item in items)
            {
                string name = item.Element("name").Value;
                string label = item.Element("label").Value;

                ServiceController service = services.FirstOrDefault(s => s.ServiceName == name);

                Button button = new Button();
                button.Name = "ServiceButton_" + i.ToString();
                button.Content = label;
                button.ToolTip = name;
                button.Margin = new Thickness(10);

                checkService(ref service, ref button);

                button.SetValue(Grid.RowProperty, i);

                RowDefinition row = new RowDefinition();
                MainGrid.RowDefinitions.Add(row);
                MainGrid.Children.Add(button);

                button.Click += (sender, args) =>
                {
                    runService(ref service, ref button);
                    checkService(ref service, ref button);
                };

                i++;
                MainWindow1.Height = 50 * i;
            }
        }

        private void checkService(ref ServiceController service, ref Button button)
        {
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending)
                {
                    button.Background = Brushes.Green;
                }
                else
                {
                    button.Background = Brushes.Red;
                }
            }else
            {
                button.IsEnabled = false;
            }
        }

        private void runService(ref ServiceController service, ref Button button)
        {
            if (!(service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending))
            {
                button.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 10));
                Mouse.OverrideCursor = null;
                button.IsEnabled = true;
            }
        }

    }
}
