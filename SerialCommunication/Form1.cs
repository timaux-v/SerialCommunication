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

            // initialize timers for Oefening 3, 4 and 5
            timerOefening3 = new System.Windows.Forms.Timer();
            timerOefening3.Interval = 1000;
            timerOefening3.Enabled = false;
            timerOefening3.Tick += timerOefening3_Tick;

            timerOefening4 = new System.Windows.Forms.Timer();
            timerOefening4.Interval = 1000;
            timerOefening4.Enabled = false;
            timerOefening4.Tick += timerOefening4_Tick;

            timerOefening5 = new System.Windows.Forms.Timer();
            timerOefening5.Interval = 1000;
            timerOefening5.Enabled = false;
            timerOefening5.Tick += timerOefening5_Tick;

            // ensure tabControl selection change is handled
            tabControl.SelectedIndexChanged += tabControl_SelectedIndexChanged;
            // synchronise timer enabled state with current selected tab at startup
            tabControl_SelectedIndexChanged(this, EventArgs.Empty);
        }

        // Serial port used to communicate with Arduino. Timeouts set to 1000ms.
        private readonly SerialPort serialPortArduino = new SerialPort()
        {
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };

        private System.Windows.Forms.Timer timerOefening3;
        private System.Windows.Forms.Timer timerOefening4;
        private System.Windows.Forms.Timer timerOefening5;
        private readonly ConcurrentQueue<int> pendingPinRequests = new ConcurrentQueue<int>();
        private readonly ConcurrentQueue<int> pendingAnalogRequests = new ConcurrentQueue<int>();
        // last received analog readings (nullable) — updated from DataReceived handler
        private int? lastAnalog0 = null;
        private int? lastAnalog1 = null;

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
            // enable timers when their respective tabs are selected
            timerOefening3.Enabled = (tabControl.SelectedTab == tabPageOefening3);
            DebugLog("Timer3 enabled: " + timerOefening3.Enabled);
            timerOefening4.Enabled = (tabControl.SelectedTab == tabPageOefening4);
            DebugLog("Timer4 enabled: " + timerOefening4.Enabled);
            timerOefening5.Enabled = (tabControl.SelectedTab == tabPageOefening5);
            DebugLog("Timer5 enabled: " + timerOefening5.Enabled);
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

        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                DebugLog("timerOefening4 tick");
                if (serialPortArduino.IsOpen)
                {
                    DebugLog("serial port is open for analog read");
                    // clear any complete-line buffer but keep fragments
                    if (serialReceiveBuffer.Length > 0) serialReceiveBuffer.Clear();

                    // remember we expect an analog response for a0
                    pendingAnalogRequests.Enqueue(0);

                    // request analog 0 value; DataReceived will populate lastAnalog0 and labelAnalog0
                    string cmd = "get a0";
                    serialPortArduino.WriteLine(cmd);
                    DebugLog("Sent: " + cmd);

                    // do not block here; DataReceived will update labelAnalog0 when response arrives
                }
                else
                {
                    DebugLog("serial port is NOT open when timer ticked");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading analog0: " + ex.Message);
            }
        }

        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            try
            {
                DebugLog("timerOefening5 tick");

                if (!serialPortArduino.IsOpen)
                {
                    DebugLog("serial port not open for Oefening5");
                    this.BeginInvoke((Action)(() =>
                    {
                        labelGewensteTemp.Text = "N/A";
                        labelHuidigeTemp.Text = "N/A";
                    }));
                    // ensure LED off
                    try { serialPortArduino.WriteLine("set d2 low"); } catch { }
                    return;
                }

                // request analog readings for a0 and a1; DataReceived will map values into lastAnalog0/1
                if (serialReceiveBuffer.Length > 0) serialReceiveBuffer.Clear();
                pendingAnalogRequests.Enqueue(0);
                serialPortArduino.WriteLine("get a0");
                DebugLog("Sent: get a0");
                pendingAnalogRequests.Enqueue(1);
                serialPortArduino.WriteLine("get a1");
                DebugLog("Sent: get a1");

                // give device time to respond
                System.Threading.Thread.Sleep(250);

                int? rawDesired = lastAnalog0;
                int? rawCurrent = lastAnalog1;

                if (!rawDesired.HasValue || !rawCurrent.HasValue)
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        if (!rawDesired.HasValue) labelGewensteTemp.Text = "N/A";
                        if (!rawCurrent.HasValue) labelHuidigeTemp.Text = "N/A";
                    }));
                    return;
                }

                double slopeDesired = (45.0 - 5.0) / 1023.0; // 40/1023
                double desiredTemp = rawDesired.Value * slopeDesired + 5.0;

                double slopeCurrent = 500.0 / 1023.0;
                double currentTemp = rawCurrent.Value * slopeCurrent;

                double desiredRounded = Math.Round(desiredTemp, 1);
                double currentRounded = Math.Round(currentTemp, 1);

                this.BeginInvoke((Action)(() =>
                {
                    labelGewensteTemp.Text = desiredRounded.ToString("F1") + " °C";
                    labelHuidigeTemp.Text = currentRounded.ToString("F1") + " °C";
                }));

                // LED control: turn on when current < desired
                try
                {
                    if (currentTemp < desiredTemp)
                    {
                        serialPortArduino.WriteLine("set d2 high");
                        DebugLog("Set d2 high");
                    }
                    else
                    {
                        serialPortArduino.WriteLine("set d2 low");
                        DebugLog("Set d2 low");
                    }
                }
                catch (Exception ex)
                {
                    DebugLog("Error setting d2: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Oefening5 timer: " + ex.Message);
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

        private StringBuilder serialReceiveBuffer = new StringBuilder();

        private void SerialPortArduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string chunk = serialPortArduino.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                DebugLog("Received raw (chunk): " + chunk.Trim());

                // accumulate chunk into buffer and extract complete lines ending with \r or \n
                serialReceiveBuffer.Append(chunk);
                var all = serialReceiveBuffer.ToString();
                // determine if the buffer ends with a newline (CR or LF)
                bool endsWithNewline = all.EndsWith("\n") || all.EndsWith("\r");
                var parts = all.Split(new[] { '\r', '\n' });
                int countToProcess = endsWithNewline ? parts.Length : Math.Max(0, parts.Length - 1);
                var linesToProcess = parts.Take(countToProcess).Where(s => !string.IsNullOrEmpty(s)).ToArray();

                // keep leftover (incomplete last part) in buffer
                serialReceiveBuffer.Clear();
                if (!endsWithNewline && parts.Length > 0)
                {
                    serialReceiveBuffer.Append(parts.Last());
                }

                if (linesToProcess.Length == 0) return;

                this.BeginInvoke((Action)(() =>
                {
                    // update status label with a preview of the last complete lines
                    labelStatus.Text = "Received: " + string.Join(" ", linesToProcess).Replace("\r", " ").Replace("\n", " ").Trim();

                    foreach (var line in linesToProcess)
                    {
                        var l = line.Trim();
                        // try parse lines like d5:1 or digital5:1
                        var m = Regex.Match(l, @"\b(?:d|digital)?\s*([5-7])\s*[:=]\s*([01])", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            int pin = int.Parse(m.Groups[1].Value);
                            bool state = m.Groups[2].Value == "1";
                            if (pin == 5) radioButtonDigital5.Checked = state;
                            else if (pin == 6) radioButtonDigital6.Checked = state;
                            else if (pin == 7) radioButtonDigital7.Checked = state;
                            continue;
                        }

                        // try parse analog like a0:123 or analog0=123 (capture pin)
                        var ma = Regex.Match(l, @"\b(?:a|analog)?\s*([01])\s*[:=]\s*(\d{1,4})", RegexOptions.IgnoreCase);
                        if (ma.Success)
                        {
                            int apin = int.Parse(ma.Groups[1].Value);
                            var val = ma.Groups[2].Value;
                            DebugLog($"Parsed analog a{apin}: {val}");
                            if (apin == 0)
                            {
                                lastAnalog0 = int.Parse(val);
                                labelAnalog0.Text = val;
                            }
                            else if (apin == 1)
                            {
                                lastAnalog1 = int.Parse(val);
                                // update raw numeric display; timer will compute scaled value for Oefening5
                                labelHuidigeTemp.Text = val;
                            }
                            continue;
                        }

                        // try bare numeric (could be mapped to pending analog or a pending digital request)
                        var mNum = Regex.Match(l, "^\\s*(\\d{1,4})\\s*$");
                        if (mNum.Success)
                        {
                            // first try map to a pending digital pin request
                            int pin;
                            if (pendingPinRequests.TryDequeue(out pin))
                            {
                                bool state = mNum.Groups[1].Value == "1";
                                DebugLog($"Mapped unlabelled response for d{pin}: {mNum.Groups[1].Value}");
                                if (pin == 5) radioButtonDigital5.Checked = state;
                                else if (pin == 6) radioButtonDigital6.Checked = state;
                                else if (pin == 7) radioButtonDigital7.Checked = state;
                                continue;
                            }

                            // else map to pending analog request if present
                            int analogPin;
                            if (pendingAnalogRequests.TryDequeue(out analogPin))
                            {
                                var val = mNum.Groups[1].Value;
                                DebugLog($"Mapped unlabelled analog response for a{analogPin}: {val}");
                                if (analogPin == 0)
                                {
                                    lastAnalog0 = int.Parse(val);
                                    labelAnalog0.Text = val;
                                }
                                else if (analogPin == 1)
                                {
                                    lastAnalog1 = int.Parse(val);
                                    labelHuidigeTemp.Text = val;
                                }
                                continue;
                            }
                        }

                        DebugLog("Unparsed line: " + l);
                    }
                }));
            }
            catch (Exception ex)
            {
                DebugLog("DataReceived error: " + ex.Message);
            }
        }
    }
}
