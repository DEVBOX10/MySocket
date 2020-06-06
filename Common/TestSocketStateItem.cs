using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// State object for receiving data from remote device.
    /// </summary>
    public class TestSocketStateItem
    {
        // Client socket.
        public Socket StateSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder ResponseText = new StringBuilder();
    }
}
