using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketServerTest
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
        }

        private string _pathClient => Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "File", "Client");
        private string _pathServer => Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "File", "Server");

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var di = new DirectoryInfo(_pathServer);
            di.Create();

            di = new DirectoryInfo(_pathClient);
            di.Create();

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = _pathServer;
            watcher.Filter = "*.txt";
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var txt = Util.ReadFile(e.FullPath);
            this.txtResult.Text += "\r\n" + txt;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                var ip = this.txtIP.Text.Trim();
                var port = Convert.ToInt32(this.txtPort.Text.Trim());
            }
            catch(Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var txt = this.txtMessage.Text.Trim();
                Util.WriteFile(txt, _pathClient);
            }
            catch (Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
            }
        }
    }
}
