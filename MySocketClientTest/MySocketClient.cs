using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySocketClientTest
{
    class MySocketClient
    {
        private Socket _Socket = null;

        public string ServerIP { get; set; }
        public int Port { get; set; }

        public bool Connected { get; set; }

        public MySocketClient()
        {

        }
        public MySocketClient(string ip, int port)
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
                // Sync 방식의 Receive 호출에 대한 완료 제한 시간 설정.
                this._Socket.ReceiveTimeout = 3 * 60 * 1000;

                this._Socket.Connect(ipEndPoint);

                this.Connected = this._Socket.Connected;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public void Release()
        {
            try
            {
                this._Socket.Shutdown(SocketShutdown.Both);
                this._Socket.Close();
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

        public void SendText(string text)
        {
            try
            {
                if (this._Socket.Connected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(text);
                    MemoryStream memoryStream = new MemoryStream(text.Length);
                    memoryStream.Write(buffer, 0, buffer.Length);
                    byte[] sendBuffer = memoryStream.GetBuffer();

                    this._Socket.Send(sendBuffer);

                    Thread.Sleep(1000);

                    this._Socket.Receive(buffer);
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
    }
}
