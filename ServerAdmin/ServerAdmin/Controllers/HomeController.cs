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
using ServerAdmin.Library;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace ServerAdmin.Controllers
{
    public class HomeController : Controller
    {
        private TcpClient tcpClient = new TcpClient();

        /// <summary>
        /// Login Homepage
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
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
            User currentUser = new User
            {
                Username = username,
                Password = password,
                IpAddress = ipAddress
            };

            string json = "";
            //Checks that the ipAddress was enterred
            if (ipAddress != null && ipAddress != "")
                json = ServerComm.ConnectToServer(tcpClient, ipAddress, username, password, "Login");

            //If json is not empty, means that it can go to the spreadsheet list without issue
            if (!json.Substring(0,5).Contains("Error"))
            {
                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));
                return RedirectToAction("SpreadsheetList", new { currentUser = currentUser });
            }
            //Must go back here
            else
            {
                ViewBag.ErrorMessage = json;
                return View();
            }
        }

        /// <summary>
        /// Webpage displaying list of spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public IActionResult SpreadsheetList(User currentUser)
        {
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index");
            }

            String username = currentUser.Username;
            String password = currentUser.Password;
            String ipAddress = currentUser.IpAddress;

            string response = "";
            SpreadsheetList spreadsheetList;
            if (ipAddress != null && ipAddress != "")
                response = ServerComm.ConnectToServer(tcpClient, ipAddress, username, password, "Login");
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Error Occured, Redirecting to Sign In");
                return RedirectToAction("Index");
            }
  
            if (!response.Substring(0, 5).Contains("Error"))
            {
                //Reads the JSON
                spreadsheetList = ReadSpreadsheetList(response);
                //If spreadsheet is not null, means successful data parsing
                if (spreadsheetList != null)
                    return View(spreadsheetList);

                //If spreadsheet is null, means server gave the wrong data for gui to read
                else
                {
                    HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                    return RedirectToAction("Index");
                }
            }
            else
            {
                HttpContext.Session.SetString("ErrorMessage", response);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Initial Get Call to create a new spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateSpread(User currentUser)
        {
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (ViewBag.ErrorMessage != null)
                return RedirectToAction("Index");

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
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, spread.Name, null, "CreateSpread");
            if (response.Substring(0, 5).Contains("Error"))
            {
                HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                return RedirectToAction("Index");
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
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (ViewBag.ErrorMessage != null)
                return RedirectToAction("Index");

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
            string response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, spread.Name, null, "DeleteSpread");
            if (response.Substring(0, 5).Contains("Error"))
            {
                HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                return RedirectToAction("Index");
            }
            return RedirectToAction("SpreadsheetList", new { currentUser });
        }

        /// <summary>
        /// Log out the user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
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
                if (value.type == "spread")
                    return value;
                else
                    throw new Exception();
            }
            catch (Exception e)
            {
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

