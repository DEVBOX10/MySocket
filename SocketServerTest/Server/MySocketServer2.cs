using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace SocketServerTest
{
    public class MySocketServer2
    {
        private Socket _ServerSocket = null;

        private Socket _AsyncAcceptSocket = null;

        List<Socket> _ClientSocketList = new List<Socket>();

        /// <summary>
        /// Thread signal.
        /// 대기 중인 스레드에 이벤트 발생 알림.
        /// </summary>
        public static ManualResetEvent _AcceptDone = new ManualResetEvent(false);

        /// <summary>
        /// Send 완료 이벤트 발생 알림.
        /// </summary>
        private static ManualResetEvent _SendDone = new ManualResetEvent(false);
        /// <summary>
        /// Receive 완료 이벤트 발생 알림.
        /// </summary>
        private static ManualResetEvent _ReceiveDone = new ManualResetEvent(false);

        // Create the server channel.
        IpcChannel _IPCServer = new IpcChannel("localhost:9999");

        public string GateServerIP { get; set; }
        public int Port { get; set; }

        public bool Connected { get; set; }

        MyLogger _Logger = new MyLogger();


        public Action<object, TestEventArgs> _ActionEventSender;
        TestEventArgs _Event = new TestEventArgs();

        public MySocketServer2()
        {
            InitLogger();
            //SetIPCServer();
            //StartPipe("GatePipe");
        }

        private void _Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_ActionEventSender != null)
                _ActionEventSender(null, _Event);
        }

        private void InitLogger()
        {
            this._Logger.LogFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "File", "Server");
            this._Logger.LogName = "RCV";
        }
        public MySocketServer2(string ip, int port)
        {
            this.GateServerIP = ip;
            this.Port = port;
            InitLogger();
        }

        public void Start()
        {
            try
            {
                //IPHostEntry host = Dns.Resolve(Dns.GetHostName());
                //string ip = host.AddressList[0].ToString();
                IPAddress ipAddress = IPAddress.Parse(this.GateServerIP);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, this.Port);

                this._ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._ServerSocket.Bind(ipEndPoint);
                this._ServerSocket.Listen(20);
                this.Connected = this._ServerSocket.Connected;

                var text = "Waiting for a connection...";
                _Logger.WriteLog(text);
                _Event.Text = text;
                //_SocketWorker.RunWorkerAsync();
                _ActionEventSender.Invoke(null, _Event);


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
                byte[] sendBuffer = new byte[10240];

                while (true)
                {
                    // Set the event to nonsignaled state.
                    _AcceptDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    //Console.WriteLine("Waiting for a connection...");

                    this._ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), this._ServerSocket);


                    //TODO: 소켓 서버에서 클라이언트 연결시도에 대한 수락 시작 이벤트 발생시점(인증처리도 함께 고려??)에 스레드 차단 고려 필요!, WaitOne()
                    // Wait until a connection is made before continuing.
                    _AcceptDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                this._Logger.WriteLog("[오류:" + "\t" + ex.Message + "]");
                throw ex;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            _AcceptDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            //_AsyncAcceptSocket = handler;

            _ClientSocketList.Add(handler);

            _Event.Text = "client connected...";
            //_SocketWorker.RunWorkerAsync();
            _ActionEventSender.Invoke(null, _Event);

            // Create the state object.
            TestSocketStateItem state = new TestSocketStateItem();
            state.StateSocket = handler;
            handler.BeginReceive(state.Buffer, 0, TestSocketStateItem.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);

            _ReceiveDone.WaitOne();
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String result = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            TestSocketStateItem state = (TestSocketStateItem)ar.AsyncState;
            Socket handler = state.StateSocket;

            _AsyncAcceptSocket = handler;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.ResponseText.Append(Encoding.UTF8.GetString(state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                result = state.ResponseText.ToString();

                //TODO: 수신된 내용에 대한 완료여부 flag (데이터 종료문자 / byte 길이) 기준 확인 필요!!!
                if (result.IndexOf("^") > -1) // <EOF>
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    var text = string.Format("Read {0} bytes from socket. \n Data : {1}", result.Length, result);
                    //Console.WriteLine(text);
                    _Event.Text = text;
                    //_SocketWorker.RunWorkerAsync(); // Additional information: This BackgroundWorker is currently busy and cannot run multiple tasks concurrently.
                    _ActionEventSender.Invoke(null, _Event);

                    text = string.Format("[수신메시지: \r\n{0}\r\n]", result.Replace("^", ""));
                    _Logger.WriteLog(text);
                    _Event.Text = text;
                    //_SocketWorker.RunWorkerAsync();
                    _ActionEventSender.Invoke(null, _Event);

                    _ReceiveDone.Set(); 
                    
                    // Echo the data back to the client.
                    //Send(handler, result);
                    //_SendDone.WaitOne();

                    TestSocketStateItem item = new TestSocketStateItem();
                    item.StateSocket = handler;

                    handler.BeginReceive(item.Buffer, 0, TestSocketStateItem.BufferSize, 0,
                    new AsyncCallback(ReadCallback), item);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.Buffer, 0, TestSocketStateItem.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }


        public void Send(Socket handler, String data)
        {
            // TODO: BeginSend(), string -> byte[] 로 변환처리 유무 고려???
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            MemoryStream memoryStream = new MemoryStream(data.Length);
            memoryStream.Write(buffer, 0, buffer.Length);
            byte[] sendBuffer = memoryStream.GetBuffer();

            TestSocketStateItem state = new TestSocketStateItem();
            state.StateSocket = handler;
            state.Buffer = sendBuffer;

            // Begin sending the data to the remote device.
            handler.BeginSend(sendBuffer, 0, sendBuffer.Length, 0,
                new AsyncCallback(SendCallback), state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                //Socket handler = (Socket)ar.AsyncState;

                TestSocketStateItem state = (TestSocketStateItem)ar.AsyncState;
                Socket handler = state.StateSocket;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);

                var text = string.Format("Sent {0} bytes to client.", bytesSent);
                //Console.WriteLine(text);
                _Event.Text = text;
                _ActionEventSender.Invoke(null, _Event);


                //TODO: Async방식의 상태 개체에서 소켓의 Release 시점 여부 고려!!!
                //Release(handler);

                _SendDone.Set();

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                this._Logger.WriteLog("[오류:" + "\t" + ex.Message + "]");
            }
        }


        public void Release()
        {
            try
            {
                //TODO: 송신에 대한 callback 에 대한 상태(소켓)에 대해 Shutdown(), Close() 처리 여부 고려???
                // Close() 전에 Shutdown() 처리 필요 - Close() 이전에 모든 데이터 처리 완료.
                this._ServerSocket.Shutdown(SocketShutdown.Both);
                this._ServerSocket.Close();

                this._AsyncAcceptSocket.Shutdown(SocketShutdown.Both);
                this._AsyncAcceptSocket.Close();
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                this._Logger.WriteLog("[오류:" + "\t" + ex.Message + "]");
            }
        }
    }
}
