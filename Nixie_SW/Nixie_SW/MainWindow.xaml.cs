using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using System.IO.Ports;

namespace Nixie_SW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer = new DispatcherTimer();
        SerialPort sp = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime dt;
            dt = DateTime.Now;
            TxtClock.Text = dt.ToLongTimeString() + "   " + "   " + dt.ToLongDateString();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedComboItem = sender as ComboBox;
            string name = selectedComboItem.SelectedItem as string;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!sp.IsOpen)
            {
                try
                {
                    string portName = COM.SelectedItem as string;
                    sp.PortName = portName;
                    sp.BaudRate = 115200;
                    sp.Open();
                    Connect.Content = "Disconnect";
                    SendTime.IsEnabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Please give a valid port number or check your connection!");
                }
            } 
            else
            {
                sp.Close();
                Connect.Content = "Connect";
                SendTime.IsEnabled = false;
            }
        }

        private void SendTime_Click(object sender, RoutedEventArgs e)
        {
            sp.Write("Hello World\n");
            // TODO send AT cmd1
            // TODO wait for response OK or ERROR
            // TODO if response OK
            // TODO send AT cmd2
            // TODO wait for response OK or ERROR
            // TODO if error then thow msg
        }
    }
}
