using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketServerTest
{
    public partial class ServerForm : Form
    {
        MySocketServer _Server = new MySocketServer();
        MySocketServer2 _ServerAsync = new MySocketServer2();

        string _IPCServer = "MyIPC";


        MyLogger _Logger = new MyLogger();
        string _BasicFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "File", "Server");

        public ServerForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TextBox.CheckForIllegalCrossThreadCalls = false;
            //this._ServerCallback._ActionEventSender = new Action<object, TEventArgs>(OnActionMessage);
        }

        //private void OnActionMessage(object sender, TEventArgs e)
        //{
            
        //    // Additional information: Cross-thread operation not valid: Control 'txtResult' accessed from a thread other than the thread it was created on.

        //    //if (this.txtResult.InvokeRequired)
        //        this.txtResult.Text += "\r\n" + e.Text;
        //}

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                var ip = this.txtIP.Text.Trim();
                var port = Convert.ToInt32(this.txtPort.Text.Trim());
                _Server.GateServerIP = ip;
                _Server.Port = port;
                _Server.Start();


                _ServerAsync.GateServerIP = ip;
                _ServerAsync.Port = port;
                _ServerAsync.Start();


                //AsyncServer server = new AsyncServer();
                //server._ActionEventSender = new Action<object, GateSystemLib.TEventArgs>(OnActionMessage);
                //server.GateServerIP = ip;
                //server.Port = port;
                //server.Start();

                //CheckConnect();
            }
            catch(Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                Logging(_BasicFolder, ex.Message);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            //this._ServerCallback.Release();
        }


        void Logging(string folder, string text)
        {
            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(text))
                return;

            _Logger.LogName = "MySocket";
            _Logger.LogFolder = folder;
            _Logger.WriteLog(text);
        }


        private void CheckConnect()
        {
            if (this._ServerAsync.Connected)
                this.btnGo.Enabled = false;
            else
                this.btnGo.Enabled = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var text = this.txtMessage.Text.Trim();
                _ServerAsync.Send(null, text);

                Logging(_BasicFolder, text);
            }
            catch (Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                Logging(_BasicFolder, ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _ServerAsync.Release();
            }
            catch (Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                Logging(_BasicFolder, ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
