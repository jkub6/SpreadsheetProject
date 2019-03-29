using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class TestClient
    {
        static void Main()
        {
            Console.WriteLine("Starting client...");

            Console.Write("Host: ");
            string host = Console.ReadLine();
            Console.Write("Port: ");
            int port = int.Parse(Console.ReadLine());
            Console.Write("Message: ");
            string message = Console.ReadLine();

            Client.Client client = new Client.Client();
            client.PingCompleted += PingCompletedCallback;
            client.NetworkMessageRecieved += printText;
            client.Connect(host, port);
            Console.WriteLine("Connected to " + host);
            client.SendNetworkMessage(message);
            Console.WriteLine("OUT: " + message);

            Console.ReadLine();
        }

        static void printText(object sender, string text)
        {
            Console.WriteLine("IN: " + text);
        }

        private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            Console.WriteLine("[Ping completed with RoundTrip time: {0}ms]", e.Reply.RoundtripTime);
        }
    }
}
