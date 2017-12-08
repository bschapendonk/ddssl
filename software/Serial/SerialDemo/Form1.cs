using System;
using System.IO.Ports;
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
            comboBox1.SelectedIndex = 0;

            FormClosing += (s, e) =>
            {
                if (serial != null && serial.IsOpen)
                {
                    serial.Write(new byte[3], 0, 3);
                    serial.Close();
                }
            };
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
    }
}
