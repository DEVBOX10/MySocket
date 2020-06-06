using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServerTest
{
    public class MySocketServer
    {
        private Socket _ServerSocket = null;
        private Socket _ClientSocket = null;

        public string GateServerIP { get; set; }
        public int Port { get; set; }

        MyLogger _Logger = new MyLogger();


        public MySocketServer()
        {
            InitLogger();
        }

        private void InitLogger()
        {
            this._Logger.LogFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "File", "Server");
            this._Logger.LogName = "RCV";
        }
        public MySocketServer(string ip, int port)
        {
            this.GateServerIP = ip;
            this.Port = port;
            InitLogger();
        }

        public void Start()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(this.GateServerIP);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, this.Port);

                this._ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._ServerSocket.Bind(ipEndPoint);
                this._ServerSocket.Blocking = true; // TODO: 서버 소켓의 blocking 옵션 고려
                this._ServerSocket.Listen(1);

                DoWork();
            }
            catch (Exception ex)
            {
                this._Logger.WriteLog("[오류:" + "\t" + ex.Message + "]");
                throw ex;
            }
            finally
            {
                //if (this._ClientSocket.Connected) 
                //    this._ClientSocket.Disconnect(true);
            }
        }

        private void DoWork()
        {
            try
            {
                byte[] headerBuffer = new byte[10];
                byte[] sendBuffer = new byte[10240];

                while (true)
                {
                    this._ClientSocket = this._ServerSocket.Accept();

                    Thread.Sleep(500);

                    byte[] receiveBuffer = new byte[10240];
                    if (this._ClientSocket.Receive(receiveBuffer) > 0)
                    {
                        string receiveMessage = System.Text.Encoding.UTF8.GetString(receiveBuffer);

                        string ackMessage = string.Empty;
                        string printMessage = string.Empty;

                        if (String.IsNullOrEmpty(receiveMessage))
                        {
                            this._Logger.WriteLog("[상태: 수신 메시지 없음! ]");
                            return;
                        }

                        //TODO: 외부로 송신이후에 응답 메시지 처리 필요???
                        ackMessage = "";


                        //TODO: !!! Sync방식에서 별도의 Thread 분리 고려 또는 Thread 분리의 제약 여부 확인???
                        sendBuffer = System.Text.Encoding.UTF8.GetBytes(ackMessage);
                        this._ClientSocket.Send(sendBuffer);
                        //this._ClientSocket.Disconnect(true);
                        Thread.Sleep(500);

                        this._ClientSocket.Shutdown(SocketShutdown.Both);
                        this._ClientSocket.Close();


                        this._Logger.WriteLog("[수신: " + receiveMessage + "]");
                        this._Logger.WriteLog("[응답: " + ackMessage + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                this._Logger.WriteLog("[오류:" + "\t" + ex.Message + "]");
                throw ex;
            }
        }


        public void Release()
        {
            try
            {
                this._ServerSocket.Shutdown(SocketShutdown.Both);
                this._ServerSocket.Close();
            }
            catch (SocketException e)
            {
                var errCode = e.ErrorCode;

                //TODO: Windows 소켓 버전 2 API 오류 코드에 대한 상세 처리 고려!!!
                switch (errCode)
                {
                    default:
                        Console.WriteLine(e.ToString());
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
