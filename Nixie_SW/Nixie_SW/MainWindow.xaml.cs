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
        SerialPort serialPort = new SerialPort();
        string[] serialPorts;
        bool serialPortOpen = false;
        DateTime dt;
        string readData;

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            Time_Update();
            InitializeSerialPorts();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
            timer.Start();
        }

        private void Time_Update()
        {
            dt = DateTime.Now;
            TxtClock.Text = dt.ToLongTimeString() + "   " + "   " + dt.ToLongDateString();

            InitializeSerialPorts();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Time_Update();
        }

        private void InitializeSerialPorts()
        {
            serialPorts = SerialPort.GetPortNames();
            COM.Items.Clear();

            if(serialPorts.Count() != 0)
            {
                foreach (string serial in serialPorts)
                {
                    COM.Items.Add(serial);
                    /*var serialItems = COM.Items;
                    if (!serialItems.Contains(serial)) // not yet in combobox
                    {
                        COM.Items.Add(serial);
                    }*/
                }

                COM.SelectedItem = serialPorts[0];  // default serial port
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if ( (COM.SelectedItem != null) && (!serialPortOpen))
            {
                try
                {
                    string selectedPortName = COM.SelectedItem.ToString();
                    serialPort.PortName = selectedPortName;
                    serialPort.BaudRate = 115200;
                    serialPort.Open();
                    Connect.Content = "Disconnect";
                    SendTime.IsEnabled = true;
                    SendNightModeEnable.IsEnabled = true;
                    SendNightModeDisable.IsEnabled = true;
                    COM.IsEnabled = false;
                    serialPortOpen = true;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("The selected serial port is busy!");
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show("There is no serial port!");
                }
                catch (Exception errorMSG)
                {
                    MessageBox.Show(errorMSG.ToString());
                }
            }
            else
            {
                if ((COM.SelectedItem == null) && COM.IsEnabled)
                {
                    MessageBox.Show("No COM port available!");
                }

                Connect.Content = "Connect";
                SendTime.IsEnabled = false;
                SendNightModeEnable.IsEnabled = false;
                SendNightModeDisable.IsEnabled = false;
                COM.IsEnabled = true;
                serialPortOpen = false;
                serialPort.Close();

            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                readData = serialPort.ReadExisting();
            }
            catch (Exception errorMSG) {
                MessageBox.Show(errorMSG.ToString());
            }
        }

        private void SendTime_Click(object sender, RoutedEventArgs e)
        {

            string cmd = "AT$DATE_TIME:" + dt.Day.ToString("00") + "-" + dt.Month.ToString("00") + "-" + dt.Year.ToString("0000") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00") + "\n";
            serialPort.Write(cmd);
            // TODO wait for response OK or ERROR
            // TODO if response OK then continue with cmd1
            // TODO if response error then throw msg

            string cmd1 = "AT$DAY:" + (int)(dt.DayOfWeek + 7) % 7 + "\n";
            serialPort.Write(cmd1);
            // TODO wait for response OK or ERROR
            // TODO if error then thow msg



        }

        private void SendNightModeEnable_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "AT$NIGHT_MODE=1\n";
            serialPort.Write(cmd);
            // TODO wait for response OK or ERROR
            // TODO if error then thow msg
        }
        private void SendNightModeDisable_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "AT$NIGHT_MODE=0\n";
            serialPort.Write(cmd);
            // TODO wait for response OK or ERROR
            // TODO if error then thow msg
        }
    }
}
