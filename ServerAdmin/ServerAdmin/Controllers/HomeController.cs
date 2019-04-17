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
using System.Threading;

namespace ServerAdmin.Controllers
{
    public class HomeController : Controller
    {
        private TcpClient tcpClient = new TcpClient();
        private User currentUser = new User();

        /// <summary>
        /// Login Homepage
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Post call for logging in with user information
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Index(String username, String password, String ipAddress)
        {
            string json = "";
            currentUser.Username = username;
            currentUser.Password = password;
            currentUser.IpAddress = ipAddress;

            //Checks that the ipAddress was enterred
            if (ipAddress != null && ipAddress != "")
                json = ConnectToServer(ipAddress, username, password, "Login");

            //If json is not empty, means that it can go to the spreadsheet list without issue
            if (json != "")
                return RedirectToAction("SpreadsheetList", new { currentUser });
            //Must go back here
            else
                return View();
        }

        /// <summary>
        /// Initial Get Call to Create User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateUser(User currentUser)
        {
            return View();
        }

        /// <summary>
        /// Post Call to Create User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateUser(User currentUser, User user)
        {
            String response = ConnectToServer(currentUser.IpAddress, user.Username, user.Password, "CreateUser");
            if (response == null)
            {
                ViewBag.ErrorMessage = "Authentication Error: Redirecting to Login";
                return RedirectToAction("Index", new { });
            }

            return RedirectToAction("SpreadsheetList", new { currentUser });
        }

        /// <summary>
        /// Initial Get Call to Delete User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteUser(User currentUser)
        {
            return View();
        }

        /// <summary>
        /// Post Call to Delete User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteUser(User currentUser, User user)
        {
            String response = ConnectToServer(currentUser.IpAddress, user.Username, user.Password, "DeleteUser");
            if (response == null)
            {
                ViewBag.ErrorMessage = "Authentication Error: Redirecting to Login";
                return RedirectToAction("Index", new { });
            }

            return RedirectToAction("SpreadsheetList", new { currentUser });
        }

        /// <summary>
        /// Initial Get Call to create a new spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateSpread(User currentUser)
        {
            return View();
        }

        /// <summary>
        /// Post Call to delete spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="spread"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateSpread(User currentUser, Spreadsheet spread)
        {
            String response = ConnectToServer(currentUser.IpAddress, spread.Name, null, "CreateSpread");
            if (response == null)
            {
                ViewBag.ErrorMessage = "Authentication Error: Redirecting to Login";
                return RedirectToAction("Index", new { });
            }

            return RedirectToAction("SpreadsheetList", new { currentUser });
        }

        /// <summary>
        /// Initial Get Call to delete spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteSpread(User currentUser)
        {
            return View();
        }

        /// <summary>
        /// Post call to delete Spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="spread"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteSpread(User currentUser, Spreadsheet spread)
        {
            string response = ConnectToServer(currentUser.IpAddress, spread.Name, null, "DeleteSpread");
            if (response == null)
            {
                ViewBag.ErrorMessage = "Authentication Error: Redirecting to Login";
                return RedirectToAction("Index", new { });
            }
            return RedirectToAction("SpreadsheetList", new { currentUser });
        }

        /// <summary>
        /// Webpage displaying list of spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IActionResult SpreadsheetList(User currentUser)
        {
            String username = currentUser.Username;
            String password = currentUser.Password;
            String ipAddress = currentUser.IpAddress;

            string json = "";
            SpreadsheetList spreadsheetList;
            if (ipAddress != null && ipAddress != "")
                json = ConnectToServer(ipAddress, username, password, "Login");

            if (json != null && json != "")
            {
                //Reads the JSON
                spreadsheetList = ReadSpreadsheetList(json);

                //If spreadsheet is not null, means successful data parsing
                if (spreadsheetList != null)
                    return View(spreadsheetList);
                //If spreadsheet is null, means server gave the wrong data for gui to read
                else
                {
                    ViewBag.ErrorMessage = "Invalid Data from Server";
                    return RedirectToAction("Index");
                }
            }
            //If json is null, something went wrong in ConnectingToServer
            else
                return RedirectToAction("Index", new { });
        }


        /// <summary>
        /// Connects to the server with the ipAddress, username, and password
        /// 
        /// Returns out true for authentication if valid data was returned
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="authenticated"></param>
        public string ConnectToServer(String ipAddress, String username, String password, String request)
        {
            try
            {
                {
                    AutoResetEvent connectDone = new AutoResetEvent(false);
                    //Port should always be 2112
                    Console.WriteLine("Connecting.....");
                    tcpClient.BeginConnect(
                        ipAddress, 2112,
                        new AsyncCallback(
                            delegate (IAsyncResult ar)
                            {
                                tcpClient.EndConnect(ar);  
                                connectDone.Set();
                            }
                        ), tcpClient
                    );
                    //Fails to connect with 2000 ms
                    if (!connectDone.WaitOne(2000))
                    {
                        ViewBag.ErrorMessage = "Server Connection Timed Out";
                        return null;
                    }
                    //Succesfully connects
                    else
                    {
                        Console.WriteLine("Connected");
                        NetworkStream stream = this.tcpClient.GetStream();
                        // use the ipaddress as in the server program
                        UserRequest ur = new UserRequest
                        {
                            IpAddress = ipAddress,
                            Username = username,
                            Request = request
                        };

                        if (password != null && password != "")
                            ur.Password = password;

                        //Sends the JSON Request to the server
                        string message = JsonConvert.SerializeObject(ur) + '\n' + '\n';

                        // Translate the passed message into ASCII and store it as a Byte array.
                        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                        // Get a client stream for reading and writing. 
                        //NetworkStream stream = tcpClient.GetStream();

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
                        {
                            return responseData;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Login Credentials not valid";
                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = "Failed to Connect to Server";
                return null;
            }
        }


        /// <summary>
        /// Parses the json string for the spreadsheet list
        /// </summary>
        /// <param name="json"></param>
        private SpreadsheetList ReadSpreadsheetList(String json)
        {
            try
            {
                //Parses the json here
                SpreadsheetList value = JsonConvert.DeserializeObject<SpreadsheetList>(json);
                if (value.type == "list")
                    return value;
                else
                    throw new Exception();
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Invalid Message Received";
                return null;
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

