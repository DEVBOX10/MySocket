using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace SocketClientTest
{
    class MySocketClient2
    {
        private Socket _Socket = null;

        /// <summary>
        /// Connect 완료 이벤트 발생 알림.
        /// ManualResetEvent instances signal completion.
        /// 대기 중인 스레드에 이벤트 발생 알림.
        /// </summary>
        private static ManualResetEvent _ConnectDone = new ManualResetEvent(false);
        /// <summary>
        /// Send 완료 이벤트 발생 알림.
        /// </summary>
        private static ManualResetEvent _SendDone = new ManualResetEvent(false);
        /// <summary>
        /// Receive 완료 이벤트 발생 알림.
        /// </summary>
        private static ManualResetEvent _ReceiveDone = new ManualResetEvent(false);

        // The response from the remote device.
        private static String _ResponseText = String.Empty;

        public string ServerIP { get; set; }
        public int Port { get; set; }

        public bool Connected { get; set; }

        public MySocketClient2()
        {
        }
        public MySocketClient2(string ip, int port)
        {
            this.ServerIP = ip;
            this.Port = port;
        }


        public void SendObject(object transfer)
        {
            try
            {
                if (this._Socket.Connected)
                {
                    byte[] receiveBuffer = new byte[5012];
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream(5012);
                    binaryFormatter.Serialize(memoryStream, transfer);
                    byte[] sendBuffer = memoryStream.GetBuffer();
                    this._Socket.Send(sendBuffer);

                    Thread.Sleep(1000);

                    this._Socket.Receive(receiveBuffer);
                    //this._Socket.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //if (this._Socket.Connected) 
                //    this._Socket.Disconnect(true);
            }
        }


        public void Connect()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(this.ServerIP), this.Port);
                this._Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                this._Socket.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallback), this._Socket);
                _ConnectDone.WaitOne();

                this.Connected = this._Socket.Connected;


            }
            catch (Exception ex)
            {
                Release();
                //throw ex;
            }
        }

        public void Release()
        {
            try
            {
                //TODO: 송신에 대한 callback 에 대한 상태(소켓)에 대해 Shutdown(), Close() 처리 여부 고려???
                // Close() 전에 Shutdown() 처리 필요 - Close() 이전에 모든 데이터 처리 완료.
                this._Socket.Shutdown(SocketShutdown.Both);
                this._Socket.Close();
                this.Connected = this._Socket.Connected;
            }
            catch (SocketException e)
            {
                var errCode = e.ErrorCode;

                //TODO: Windows 소켓 버전 2 API 오류 코드에 대한 상세 처리 고려!!!
                switch (errCode)
                {
                    default:
                        Console.WriteLine(e.Message);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = new byte[1024];


                Socket client = (Socket)ar.AsyncState;

                //TODO: ConnectCallback에서 EndConnect() ???
                client.EndConnect(ar);

                TestSocketStateItem state = new TestSocketStateItem();
                state.StateSocket = client;
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);

                var text = string.Format("Socket connected to {0}", client.RemoteEndPoint.ToString());

                //Console.WriteLine(text);

                // Signal that the connection has been made.
                _ConnectDone.Set();

            }
            catch (Exception e)
            {
                Release();
            }
        }


        public void SendText(string text)
        {
            try
            {
                // TODO: BeginSend(), string -> byte[] 로 변환처리 유무 고려???
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                MemoryStream memoryStream = new MemoryStream(text.Length);
                memoryStream.Write(buffer, 0, buffer.Length);
                byte[] sendBuffer = memoryStream.GetBuffer();

                // Begin sending the data to the remote device.
                this._Socket.BeginSend(sendBuffer, 0, sendBuffer.Length, 0,
                    new AsyncCallback(SendCallback), this._Socket);

                //TODO: Send 완료 이벤트 발생시점에 스레드 차단 시점 고려 필요!, WaitOne() - block thread until wait handle receives a signal, 어느 시점에 적용할지 고려???!!!
                _SendDone.WaitOne();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //if (this._Socket.Connected) 
                //    this._Socket.Disconnect(true);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);

                var text = string.Format("Sent {0} bytes to server.", bytesSent);
                //Console.WriteLine(text);

                // Signal that all bytes have been sent.
                _SendDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                throw e;
            }
        }


        public void ReceiveText(string text)
        {
            try
            {
                TestSocketStateItem state = new TestSocketStateItem();
                state.StateSocket = this._Socket;

                // Begin receiving the data from the remote device.
                this._Socket.BeginReceive(state.Buffer, 0, TestSocketStateItem.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);

                //TODO: Receive 완료 이벤트 발생시점에 스레드 차단 시점 고려 필요!, WaitOne() - block thread until wait handle receives a signal, 어느 시점에 적용할지 고려???!!!
                _ReceiveDone.WaitOne();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //if (this._Socket.Connected) 
                //    this._Socket.Disconnect(true);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                String result = String.Empty;
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                TestSocketStateItem state = (TestSocketStateItem)ar.AsyncState;
                Socket client = state.StateSocket;

                //Socket client = ar.AsyncState as Socket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.ResponseText.Append(Encoding.UTF8.GetString(state.Buffer, 0, state.Buffer.Length));

                    result = state.ResponseText.ToString();

                    if (result.IndexOf("^") > -1)
                    {
                        var text = string.Format("Read {0} bytes from server. \n Data : {1}", result.Length, result);
                    }
                    // Get the rest of the data.
                    client.BeginReceive(state.Buffer, 0, TestSocketStateItem.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.ResponseText.Length > 1)
                    {
                        _ResponseText = state.ResponseText.ToString();
                    }
                    // Signal that all bytes have been received.
                    _ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                throw e;
            }
        }
    }
}
