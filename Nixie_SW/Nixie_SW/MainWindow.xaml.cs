using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.IO.Ports;

namespace Nixie_SW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        readonly DispatcherTimer timer1s = new DispatcherTimer();
        readonly DispatcherTimer timer5s = new DispatcherTimer();
        readonly SerialPort serialPort = new SerialPort();
        string[] serialPorts;
        DateTime dt;
        string readData;
        Operation operation;
        int timeout;
        const int TIMEOUT_SEC = 2;

        public MainWindow()
        {
            InitializeComponent();
            timer1s.Tick += new EventHandler(Timer1s_Tick);
            timer1s.Interval = new TimeSpan(0, 0, 1);

            TimeUpdate();
            InitializeSerialPorts();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
            timer1s.Start();
        }

        private void Timer1s_Tick(object sender, EventArgs e)
        {
            timer1s.Stop();

            TimeUpdate();
            InitializeSerialPorts();

            if (operation != Operation.NotSet)
            {
                timeout++;
            }
            
            if(timeout >= TIMEOUT_SEC)
            {
                MessageBox.Show("Command timeout!");
                operation = Operation.NotSet;
                timeout = 0;
            }

            timer1s.Start();
        }

        private void TimeUpdate()
        {
            dt = DateTime.Now;
            TxtClock.Text = dt.ToLongTimeString() + "   " + "   " + dt.ToLongDateString();
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
                }

                COM.SelectedItem = serialPorts[0];  // default serial port
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if ( (COM.SelectedItem != null) && (!serialPort.IsOpen))
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
                    MessageBox.Show(errorMSG.Message);
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
                LogOutput.Clear();
                serialPort.Close();

            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                bool status = false;
                readData = serialPort.ReadExisting();
                LogOutput.Text += "Read: " + readData;

                if (readData.Contains("OK"))
                    status = true;
                else if (readData.Contains("ERROR"))
                {
                    MessageBox.Show("Device responded with error for command!");
                }
                else
                {
                    MessageBox.Show("Unexpected response for command!");
                }
                    
                switch (operation)
                {
                    case Operation.SendDateTime:
                        if(status)
                        {
                            operation = Operation.SendDay;
                            string cmd = "AT$DAY:" + (int)(dt.DayOfWeek + 7) % 7 + "\n";
                            serialPort.Write(cmd);
                            LogOutput.Text += "Write: " + cmd;
                        } 
                        else
                        {
                            operation = Operation.NotSet;
                        }
                        
                        break;

                    case Operation.SendDay:
                        operation = Operation.NotSet;
                        break;

                    case Operation.EnableNightMode:
                        operation = Operation.NotSet;
                        break;

                    case Operation.DisableNightMode:
                        operation = Operation.NotSet;
                        break;
                }
            }
            catch (Exception errorMSG) {
                MessageBox.Show(errorMSG.Message);
            }
        }

        private void SendTime_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }

            string cmd = "AT$DATE_TIME:" + dt.Day.ToString("00") + "-" + dt.Month.ToString("00") + "-" + dt.Year.ToString("0000") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00") + "\n";
            operation = Operation.SendDateTime;
            serialPort.Write(cmd);
            LogOutput.Text += "Write: " + cmd;
        }

        private void SendNightModeEnable_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }

            string cmd = "AT$NIGHT_MODE=1\n";
            operation = Operation.EnableNightMode;
            serialPort.Write(cmd);
            LogOutput.Text += "Write: " + cmd;
        }

        private void SendNightModeDisable_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                return;
            }

            string cmd = "AT$NIGHT_MODE=0\n";
            operation = Operation.DisableNightMode;
            serialPort.Write(cmd);
            LogOutput.Text += "Write: " + cmd;
        }

        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }

    public enum Operation
    { 
        NotSet,
        SendDateTime,   // cmd
        SendDay,        // cmd1
        EnableNightMode,
        DisableNightMode
    }
}
