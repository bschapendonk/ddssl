using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDSSLTray
{
    public class DDSSLTrayContex : ApplicationContext
    {
        private readonly string connectionstring = ConfigurationManager.AppSettings["ddssl:connectionstring"];
        private readonly string deviceid = ConfigurationManager.AppSettings["ddssl:deviceid"];

        private readonly ServiceClient serviceClient;

        public DDSSLTrayContex()
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionstring);
            var notifyIcon = new NotifyIcon();

            var red = new ToolStripMenuItem("Red");
            red.Image = Properties.Resources.Red;
            red.Click += async (sender, e) => await SendToDevice(false, false, true);

            var orange = new ToolStripMenuItem("Orange");
            orange.Image = Properties.Resources.Orange;
            orange.Click += async (sender, e) => await SendToDevice(false, true, false);

            var green = new ToolStripMenuItem("Green");
            green.Image = Properties.Resources.Green;
            green.Click += async (sender, e) => await SendToDevice(true, false, false);

            var off = new ToolStripMenuItem("Off");
            off.Image = Properties.Resources.Off;
            off.Click += async (sender, e) => await SendToDevice(false, false, false);

            var exit = new ToolStripMenuItem("Exit");
            exit.Image = Properties.Resources.Exit;
            exit.Click += async (sender, e) =>
            {
                notifyIcon.Visible = false;
                await SendToDevice(false, false, false);
                Application.Exit();
            };

            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(red);
            contextMenuStrip.Items.Add(orange);
            contextMenuStrip.Items.Add(green);
            contextMenuStrip.Items.Add(off);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(exit);

            notifyIcon.Icon = Properties.Resources.AppIcon;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Visible = true;
        }

        private async Task SendToDevice(bool green, bool orange, bool red)
        {
            var json = JsonConvert.SerializeObject(new
            {
                Name = "brightness",
                Parameters = new
                {
                    green = green ? 255 : 0,
                    orange = orange ? 255 : 0,
                    red = red ? 255 : 0
                }
            });
            var bytes = Encoding.UTF8.GetBytes(json);
            var message = new Microsoft.Azure.Devices.Message(bytes);
            await serviceClient.SendAsync(deviceid, message);
        }
    }
}
