using NAudio.CoreAudioApi;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace SerialDemo
{
    public partial class Form1 : Form
    {
        private SerialPort serial;

        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }

            FormClosing += (s, e) =>
            {
                if (serial != null && serial.IsOpen)
                {
                    serial.Write(new byte[3], 0, 3);
                    serial.Close();
                }
            };

            var r = (1 * Math.Log10(2)) / Math.Log10(255);


            worker.DoWork += (s, e) =>
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                var buffer = new byte[3];
                while (true)
                {
                    if (serial != null && serial.IsOpen)
                    {
                        if (checkBox1.Checked)
                        {
                            var master = device.AudioMeterInformation.MasterPeakValue;

                            if (master <= (1f / 3f))
                            {

                                //buffer[0] = (byte)Math.Floor(255 * (3 * master));
                                buffer[0] = (byte)((Math.Pow(2, (3 * master) / r)) - 1);
                                buffer[1] = 0;
                                buffer[2] = 0;
                            }
                            else if (master <= (2f / 3f))
                            {
                                buffer[0] = 255;
                                //buffer[1] = (byte)Math.Floor(255 * (3 * master - (1f / 3f)));
                                buffer[1] = (byte)((Math.Pow(2, (3 * master - (1f / 3f)) / r)) - 1);
                                buffer[2] = 0;
                            }
                            else
                            {
                                buffer[0] = 255;
                                buffer[1] = 255;
                                //buffer[2] = (byte)Math.Floor(255 * (3 * master - (2f / 3f)));
                                buffer[2] = (byte)((Math.Pow(2, (3 * master - (2f / 3f)) / r)) - 1);
                            }
                            serial.Write(buffer, 0, 3);

                        }
                        Thread.Sleep(20);
                    }
                }

            };
            worker.RunWorkerAsync();
        }

        private void Send()
        {
            if (serial != null && serial.IsOpen)
            {
                var buffer = new byte[3];

                buffer[0] = (byte)green.Value;
                buffer[1] = (byte)yellow.Value;
                buffer[2] = (byte)red.Value;

                serial.Write(buffer, 0, 3);
            }
        }

        private void red_Scroll(object sender, EventArgs e)
        {
            Send();
        }

        private void yellow_Scroll(object sender, EventArgs e)
        {
            Send();
        }

        private void green_Scroll(object sender, EventArgs e)
        {
            Send();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var port = comboBox1.SelectedItem as string;
            if (serial == null && !string.IsNullOrEmpty(port))
            {
                serial = new SerialPort(port, 115200);
                serial.Open();
                if (serial.IsOpen)
                {
                    button1.Enabled = false;
                    button1.Text = "Connected";
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}
