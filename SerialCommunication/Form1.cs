using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // initialize timer for Oefening 3
            timerOefening3 = new System.Windows.Forms.Timer();
            timerOefening3.Interval = 1000;
            timerOefening3.Enabled = false;
            timerOefening3.Tick += timerOefening3_Tick;

            // ensure tabControl selection change is handled
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
        }

        // Serial port used to communicate with Arduino. Timeouts set to 1000ms.
        private readonly SerialPort serialPortArduino = new SerialPort()
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };

        private System.Windows.Forms.Timer timerOefening3;
        private readonly ConcurrentQueue<int> pendingPinRequests = new ConcurrentQueue<int>();

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (serialPortArduino.IsOpen)
            {
                // Already open — close connection
                try
                {
                    serialPortArduino.DataReceived -= SerialPortArduino_DataReceived;
                    serialPortArduino.Close();
                    radioButtonVerbonden.Checked = false;
                    buttonConnect.Text = "Connect";
                    labelStatus.Text = "Disconnected";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error closing port: " + ex.Message);
                }
            }
            else
            {
                // Not open — open connection. Read settings from UI controls.
                try
                {
                    if (comboBoxPoort.SelectedItem == null)
                    {
                        MessageBox.Show("Select a serial port first.");
                        return;
                    }

                    string portName = comboBoxPoort.SelectedItem.ToString();
                    serialPortArduino.PortName = portName;

                    int baud;
                    if (comboBoxBaudrate.SelectedItem != null && int.TryParse(comboBoxBaudrate.SelectedItem.ToString(), out baud))
                        serialPortArduino.BaudRate = baud;

                    // set DTR/RTS from UI checkboxes before opening
                    try { serialPortArduino.DtrEnable = checkBoxDtrEnable.Checked; } catch { }
                    try { serialPortArduino.RtsEnable = checkBoxRtsEnable.Checked; } catch { }

                    // use CRLF as newline to match common Arduino sketches
                    serialPortArduino.NewLine = "\r\n";

                    serialPortArduino.Open();
                    serialPortArduino.DataReceived += SerialPortArduino_DataReceived;

                    radioButtonVerbonden.Checked = true;
                    buttonConnect.Text = "Disconnect";
                    labelStatus.Text = "Connected";

                    // send a small probe to check the device responds
                    try
                    {
                        serialPortArduino.WriteLine("ping");
                        DebugLog("Sent: ping");
                    }
                    catch (Exception ex)
                    {
                        DebugLog("Probe send failed: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening port: " + ex.Message);
                }
            }
        }

        private void checkBoxDigital2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando; // set d2 high/low
                    if (checkBoxDigital2.Checked) commando = "set d2 high";
                    else commando = "set d2 low";
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening port: " + ex.Message);


            }

        }

        private void checkBoxDigital3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando; // set d3 high/low
                    if (checkBoxDigital3.Checked) commando = "set d3 high";
                    else commando = "set d3 low";
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening port: " + ex.Message);


            }
        }

        private void checkBoxDigital4_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando; // set d4 high/low
                    if (checkBoxDigital4.Checked) commando = "set d4 high";
                    else commando = "set d4 low";
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening port: " + ex.Message);


            }
        }

        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando = "set pwm9 " + trackBarPWM9.Value;
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending PWM command: " + ex.Message);
            }
        }

        private void trackBarPWM10_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando = "set pwm10 " + trackBarPWM10.Value;
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending PWM command: " + ex.Message);
            }
        }

        private void trackBarPWM11_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    string commando = "set pwm11 " + trackBarPWM11.Value;
                    serialPortArduino.WriteLine(commando);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending PWM command: " + ex.Message);
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // enable timer only when Oefening 3 tab is selected
            if (tabControl.SelectedTab == tabPageOefening3)
                timerOefening3.Enabled = true;
            else
                timerOefening3.Enabled = false;
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino.IsOpen)
                {
                    // remove any previous answers
                    serialPortArduino.ReadExisting();

                    // helper to query a digital pin and return true when response is "1"
                    Func<int, bool> readDigital = (pin) =>
                    {
                        try
                        {
                            string cmd = $"get d{pin}";
                            // remember this request so unlabelled responses can be mapped
                            pendingPinRequests.Enqueue(pin);
                            serialPortArduino.WriteLine(cmd);
                            DebugLog("Sent: " + cmd);
                            // give Arduino more time to respond
                            System.Threading.Thread.Sleep(200);
                            var resp = serialPortArduino.ReadExisting().Trim();
                            DebugLog($"Resp d{pin}: {resp}");
                            // if resp is empty, we rely on DataReceived parsing and queued mapping
                            if (string.IsNullOrEmpty(resp)) return false;
                            return resp == "1";
                        }
                        catch
                        {
                            int dropped;
                            // ensure queue doesn't grow stale
                            pendingPinRequests.TryDequeue(out dropped);
                            return false;
                        }
                    };

                    radioButtonDigital5.Checked = readDigital(5);
                    radioButtonDigital6.Checked = readDigital(6);
                    radioButtonDigital7.Checked = readDigital(7);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading digital pins: " + ex.Message);
            }
        }

        private void DebugLog(string message)
        {
            try
            {
                if (textBoxDebugLog == null) return;
                this.BeginInvoke((Action)(() =>
                {
                    textBoxDebugLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + " " + message + Environment.NewLine);
                    // keep last ~20k chars
                    if (textBoxDebugLog.TextLength > 20000)
                    {
                        textBoxDebugLog.Text = textBoxDebugLog.Text.Substring(textBoxDebugLog.TextLength - 20000);
                        textBoxDebugLog.SelectionStart = textBoxDebugLog.TextLength;
                        textBoxDebugLog.SelectionLength = 0;
                    }
                }));
            }
            catch { }
        }

        private void SerialPortArduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPortArduino.ReadExisting();
                if (!string.IsNullOrEmpty(data))
                {
                    DebugLog("Received raw: " + data.Trim());
                    this.BeginInvoke((Action)(() =>
                    {
                        // update status label with received data preview
                        labelStatus.Text = "Received: " + data.Trim().Replace("\r", " ").Replace("\n", " ").Trim();
                        // try parse lines like d5:1 or digital5:1
                        var lines = data.Split(new[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            var m = Regex.Match(line, @"\b(?:d|digital)?\s*([5-7])\s*[:=]\s*([01])", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                int pin = int.Parse(m.Groups[1].Value);
                                bool state = m.Groups[2].Value == "1";
                                if (pin == 5) radioButtonDigital5.Checked = state;
                                else if (pin == 6) radioButtonDigital6.Checked = state;
                                else if (pin == 7) radioButtonDigital7.Checked = state;
                            }
                            else
                            {
                                // try match bare '0' or '1' and map to oldest pending request
                                var m2 = Regex.Match(line, "^\\s*([01])\\s*$");
                                if (m2.Success)
                                {
                                    int pin;
                                    if (pendingPinRequests.TryDequeue(out pin))
                                    {
                                        bool state = m2.Groups[1].Value == "1";
                                        DebugLog($"Mapped unlabelled response for d{pin}: {m2.Groups[1].Value}");
                                        if (pin == 5) radioButtonDigital5.Checked = state;
                                        else if (pin == 6) radioButtonDigital6.Checked = state;
                                        else if (pin == 7) radioButtonDigital7.Checked = state;
                                    }
                                    else
                                    {
                                        DebugLog("No pending pin request to map for line: " + line);
                                    }
                                }
                                else
                                {
                                    DebugLog("Unparsed line: " + line);
                                }
                            }
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                DebugLog("DataReceived error: " + ex.Message);
            }
        }
    }
}
