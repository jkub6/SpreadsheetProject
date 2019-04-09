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
        public IActionResult Index(String username, String password, String ipAddress)
        {
            if (ipAddress != null && ipAddress != "")
                ConnectToServer(ipAddress, username, password);

            return View();
        }

        public void ConnectToServer(String ipAddress, String username, String password)
        {
            try
            {
                TcpClient tcpclnt = new TcpClient();
                Console.WriteLine("Connecting.....");

                tcpclnt.Connect(ipAddress, 2112);

                // use the ipaddress as in the server program
                Console.WriteLine("Connected");
                UserLoginRequest ulr = new UserLoginRequest
                {
                    IpAddress = ipAddress,
                    Username = username,
                    Password = password,
                    Request = "Login"
                };

                //Sends the JSON Request to the server
                string message = JsonConvert.SerializeObject(ulr);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                NetworkStream stream = tcpclnt.GetStream();

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



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
