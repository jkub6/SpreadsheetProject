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
using Microsoft.AspNetCore.Authorization;

namespace ServerAdmin.Controllers
{
    public class HomeController : Controller
    {
        private TcpClient tcpClient = new TcpClient();
        private SpreadsheetList spreadsheetList = new SpreadsheetList();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Index(String username, String password, String ipAddress)
        {
            //IP Address: 24.10.184.57
            //Port: 2112

            bool authenticated = false;

            //TODO:
            //Send protocal JSON for type
            //What is in each message for what needs to be implemented in the JSON
            if (ipAddress != null && ipAddress != "")
            {
                ConnectToServer(ipAddress, username, password, out authenticated);
            }

            if (authenticated)
                return RedirectToAction("SpreadsheetList", new { username, password, ipAddress });
            else
            {
                ViewBag.Failed = true;
                return View();
            }
        }

        public IActionResult SpreadsheetList(String username, String password, String ipAddress, bool authenticated)
        {
            //IP Address: 24.10.184.57
            //Port: 2112

            //TODO:
            //Send protocal JSON for type
            //What is in each message for what needs to be implemented in the JSON
            if (ipAddress != null && ipAddress != "")
                ConnectToServer(ipAddress, username, password, out authenticated);

            return View(spreadsheetList);
        }

        //TODO: Ping to the server
        public void ConnectToServer(String ipAddress, String username, String password, out bool authenticated)
        {
            authenticated = false;
            try
            {
                Console.WriteLine("Connecting.....");
                tcpClient.Connect(ipAddress, 2112);

                // use the ipaddress as in the server program
                Console.WriteLine("Connected");
                UserRequest ur = new UserRequest
                {
                    IpAddress = ipAddress,
                    Username = username,
                    Password = password,
                    Request = "Login"
                };

                //Sends the JSON Request to the server
                string message = JsonConvert.SerializeObject(ur) + '\n' + '\n';

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
                if (responseData != null && responseData != "")
                    authenticated = true;

                Read(responseData);
                string value = "5";
                //tcpclnt.Close();
            }
            catch (Exception e)
            {
                authenticated = false;
                ViewBag.Error = "Error..... " + e.StackTrace;
            }
        }

        private void Read(String json)
        {
            try
            //Parse message here
            {
                SpreadsheetList value = JsonConvert.DeserializeObject<SpreadsheetList>(json);
                if (value.type == "list")
                {
                    if (value.Equals(spreadsheetList))
                        return;
                    else
                        spreadsheetList = value;
                }
                else
                    throw new Exception();
            }
            catch (Exception)
            {

            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
