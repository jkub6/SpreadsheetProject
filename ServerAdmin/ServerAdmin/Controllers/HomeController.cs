using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAdmin.Models;
using Newtonsoft.Json;

namespace ServerAdmin.Controllers
{
    public class HomeController : Controller
    {
        private TcpClient tcpClient;
        public IActionResult Index(String username, String password, String ipAddress)
        {
            //IP Address: 24.10.184.57
            //Port: 2112

            //TODO:
            //Send protocal JSON for type
            //What is in each message for what needs to be implemented in the JSON
            if (ipAddress != null && ipAddress != "")
                ConnectToServer(ipAddress, username, password);

            bool authenticated = false;
            if (authenticated)
                Task.Run(() => Read());

            return View();
        }

        //TODO: Ping to the server
        public void ConnectToServer(String ipAddress, String username, String password)
        {
            try
            {
                Console.WriteLine("Connecting.....");
                tcpClient.Connect(ipAddress, 2112);

                // use the ipaddress as in the server program
                Console.WriteLine("Connected");
                UserRequest ulr = new UserRequest
                {
                    IpAddress = ipAddress,
                    Username = username,
                    Password = password,
                    Request = "Login"
                };

                //Sends the JSON Request to the server
                string message = JsonConvert.SerializeObject(ulr) + '\n' + '\n';

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                // Get a client stream for reading and writing. 
                NetworkStream stream = tcpClient.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length); //(**This is to send data using the byte method**) 
                Console.WriteLine("Transmitting.....");

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)

                string value = "5";
                //tcpclnt.Close();
            }

            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
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
                NetworkMessageRecieved(this, message);


                try
                {
                    JObject o = JObject.Parse(message);
                    string type = (string)o["type"];

                    if (type == "error")
                        ErrorRecieved(this, (int)o["code"]);
                    else if (type == "full send")
                    {
                        Spreadsheet newSpreadsheet = new Spreadsheet();
                        JToken cells = o["cells"];
                        foreach (JObject ob in cells.Children<JObject>())
                            foreach (JProperty p in ob.Properties())
                                newSpreadsheet.SetContentsOfCell(p.Name, (string)p.Value);
                        spreadsheet = newSpreadsheet;
                        FullSendRecieved(this, newSpreadsheet);
                    }

                }
                catch (Exception) { }
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
