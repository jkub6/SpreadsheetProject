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

using SpreadsheetUtilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Client
{
    public class Client
    {
        private Spreadsheet spreadsheet;

        private string host;
        private IPAddress hostIP;
        private int port;

        private TcpClient tcpClient;
        public event EventHandler<string> NetworkMessageRecieved;
        public event EventHandler<int> ErrorRecieved;
        public event EventHandler<Spreadsheet> FullSendRecieved;

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

        public void SendOpen(string spreadsheet, string username, string password)
        {
            SendNetworkMessage($"{{\"type\": \"open\",\"name\": \"{spreadsheet}\",\"username\": \"{username}\",\"password\": \"{password}\"}}");
        }

        public void SendEdit(string cell)
        {
            string v = spreadsheet.GetCellValue(cell).ToString();

            List<string> deps = new List<string>();
            if (spreadsheet.GetCellContents(cell) is Formula f)
                deps = new List<string>(f.GetVariables());

            string d = "[" + string.Join("\",\"", deps.ToArray()) + "]";

            SendNetworkMessage($"{{\"type\": \"edit\",\"cell\": \"{cell}\",\"value\": \"={v}\",\"dependencies\":{d}}}");
        }

        public void SendUndo()
        {
            SendNetworkMessage("{\"type\": \"undo\"}");
        }

        public void SendRevert(string cell)
        {
           SendNetworkMessage($"{{\"type\": \"revert\",\"cell\": \"{cell}\"}}");
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
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                NetworkMessageRecieved?.Invoke(this, message);


                try
                {
                    JObject o = JObject.Parse(message);
                    string type = (string)o["type"];

                    if (type == "error")
                        ErrorRecieved?.Invoke(this, (int)o["code"]);
                    else if (type == "full send")
                    {
                        Spreadsheet newSpreadsheet = new Spreadsheet();
                        JToken cells = o["cells"];
                        foreach (JObject ob in cells.Children<JObject>())
                            foreach (JProperty p in ob.Properties())
                                newSpreadsheet.SetContentsOfCell(p.Name, (string)p.Value);
                        spreadsheet = newSpreadsheet;
                        FullSendRecieved?.Invoke(this, newSpreadsheet);
                    }

                }
                catch (Exception) { }
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
