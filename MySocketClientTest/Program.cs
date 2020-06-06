using System;

namespace MySocketClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new MySocketClient("127.0.0.1", 1010);
            client.Connect();
        }
    }
}
