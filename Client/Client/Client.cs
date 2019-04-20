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
using System.Diagnostics;

namespace Client
{
    public class Client
    {
        public Spreadsheet spreadsheet;

        private string host;
        private IPAddress hostIP;
        private int port;

        private TcpClient tcpClient;
        public event EventHandler<string> NetworkMessageRecieved;
        public event EventHandler<int> ErrorRecieved;
        public event EventHandler FullSendRecieved;
        public event EventHandler<List<string>> SpreadsheetsRecieved;

        public event EventHandler<string> SendingText;

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
            tcpClient = new TcpClient();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            //var result = tcpClient.ConnectAsync(ipAddress, port);
            var result = tcpClient.BeginConnect(ipAddress, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5),false);
            
            if (!success)
            {
                tcpClient.Close();
                throw new Exception("Connection Timeout");
            }
            tcpClient.EndConnect(result);

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
            SendNetworkMessage($"{{\"type\": \"open\",\"name\": \"{spreadsheet}\",\"username\": \"{username}\",\"password\": \"{password}\"}}\n\n");
        }

        public void SendEdit(string cell, string contents)
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell(cell, contents);

            string c = s.GetCellContents(cell).ToString();

            List<string> deps = new List<string>();
            if (s.GetCellContents(cell) is Formula f)
            {
                deps = new List<string>(f.GetVariables());
                c = "=" + c;
            }
                

            string d = "[" + string.Join("\",\"", deps.ToArray()) + "]";

            SendNetworkMessage($"{{\"type\": \"edit\",\"cell\": \"{cell}\",\"value\": \"{c}\",\"dependencies\":{d}}}\n\n");
        }

        public void SendUndo()
        {
            SendNetworkMessage("{\"type\": \"undo\"}\n\n");
        }

        public void SendRevert(string cell)
        {
           SendNetworkMessage($"{{\"type\": \"revert\",\"cell\": \"{cell}\"}}\n\n");
        }

        public void SendNetworkMessage(string message)
        {
            if (!tcpClient.Connected)
                return;
            Stream stm = tcpClient.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(message);

            stm.Write(ba, 0, ba.Length);
            SendingText?.Invoke(this, message);
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
                        JToken cells = o["spreadsheet"];

                        var cellDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(cells.ToString());

                        foreach (string name in cellDict.Keys)
                            newSpreadsheet.SetContentsOfCell(name, cellDict[name]);
                                
                        foreach (string cellName in spreadsheet.GetNamesOfAllNonemptyCells())
                        {
                            string contents = spreadsheet.GetCellContents(cellName).ToString();
                            if (spreadsheet.GetCellContents(cellName) is Formula f)
                                contents = "=" + contents;
                            newSpreadsheet.SetContentsOfCell(cellName, contents);
                        }

                        spreadsheet = newSpreadsheet;
                        FullSendRecieved?.Invoke(this, new EventArgs());
                    }
                    else if (type == "list")
                    {
                        List<string> spreadsheets = new List<string>();
                        JToken cells = o["spreadsheets"];
                        foreach (string ob in cells)
                            spreadsheets.Add(ob.ToString());
                        SpreadsheetsRecieved?.Invoke(this, spreadsheets);
                    }
                }
                catch (Exception e) { Debug.WriteLine(e.Message); }
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
