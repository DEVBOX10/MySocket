using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketClientTest
{
    public partial class ClientForm : Form
    {
        MySocketClient _Client = new MySocketClient();
        MySocketClient2 _ClientAsync = new MySocketClient2();

        public ClientForm()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var text = this.txtMessage.Text.Trim();
                //text += "^"; // End of Text 

                //_Client.SendText(text);
                _ClientAsync.SendText(text);
            }
            catch(Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                UpdateControls();
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            TextBox.CheckForIllegalCrossThreadCalls = false;
            //this._ClientCallback._ActionSender = new Action<object, TEventArgs>(OnActionMessage);

            UpdateControls();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            //this._ClientCallback.Release();
        }
        private void btnConnet_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var ip = this.txtIP.Text.Trim();
                var port = Convert.ToInt32(this.txtPort.Text.Trim());

                //_Client.ServerIP = ip;
                //_Client.Port = port;
                //_Client.Connect();


                this._ClientAsync.ServerIP = ip;
                this._ClientAsync.Port = port;
                this._ClientAsync.Connect();
            }
            catch (Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                UpdateControls();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (this._ClientAsync.Connected)
                    this._ClientAsync.Release();

                //CheckConnect();
            }
            catch (Exception ex)
            {
                this.txtResult.Text += "\r\n" + ex.Message;
                UpdateControls();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateControls()
        {
            if (this._ClientAsync.Connected)
                this.btnConnet.Enabled = false;
            else
                this.btnConnet.Enabled = true;

        }
    }
}
