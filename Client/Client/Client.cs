using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

using System.Timers;

using SS;
using System.IO;
using System.Threading;

namespace Client
{
    public class Client
    {
        private Spreadsheet spreadsheet;

        private string host;
        private IPAddress hostIP;
        private int port;

        private TcpClient tcpClient;
        public delegate void TextHandler(string text);
        public event TextHandler NetworkMessageRecieved;

        System.Timers.Timer pingTimer;
        public event PingCompletedEventHandler PingCompleted;

        public Client()
        {
            spreadsheet = new Spreadsheet();
            tcpClient = new TcpClient();
            pingTimer = new System.Timers.Timer(1000);
        }

        public void Connect(string host, int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            tcpClient.ConnectAsync(ipAddress, port).Wait();

            this.host = host;
            this.hostIP = ipAddress;
            this.port = port;

            Task.Run(() => Read());

            pingTimer.Elapsed += SendPing;
            pingTimer.AutoReset = true;
            pingTimer.Enabled = true;
        }

        public void SendNetworkMessage(string message)
        {
            Stream stm = tcpClient.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(message);

            stm.Write(ba, 0, ba.Length);
        }

        private async Task Read()
        {
            var buffer = new byte[4096];
            var ns = tcpClient.GetStream();
            while (true)
            {
                var bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return; // Stream was closed
                NetworkMessageRecieved(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
        }

        private void SendPing(object sender, ElapsedEventArgs e)
        {
            AutoResetEvent waiter = new AutoResetEvent(false);

            Ping pingSender = new Ping();
            pingSender.PingCompleted += PingCompleted;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 12000;
            PingOptions options = new PingOptions(64, true);
            pingSender.SendAsync(hostIP, timeout, buffer, options, waiter);
        }
    }
}
