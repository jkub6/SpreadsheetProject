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
            ViewData.Clear();
            HttpContext.Session.Clear();
            User currentUser = new User
            {
                Username = username,
                Password = password,
                IpAddress = ipAddress
            };

            string json = "";
            //Checks that the ipAddress was enterred
            if (ipAddress != null && ipAddress != "")
            {
                json = ServerComm.ConnectToServer(tcpClient, ipAddress, username, password, "Login");
                //If json is not empty, means that it can go to the spreadsheet list without issue
                if (json.Length < 4 || (json.Length >= 4 && !json.Substring(0, 4).Contains("fail")))
                {
                    HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));

                    json = ServerComm.ConnectToServer(tcpClient, ipAddress, username, password, "SpreadsheetList");
                    if (!json.Substring(0, 5).Contains("Error"))
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
                
                //Password fails
                else
                {
                    ViewBag.ErrorMessage = "Error: Invalid Password";
                    return View();
                }
            }

            //Shouldn't be possible
            return View();
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
                response = ServerComm.ConnectToServer(tcpClient, ipAddress, username, password, "SpreadsheetList");
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
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index");
            }

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
        public IActionResult CreateSpread(User currentUser, string spreadsheetName, string submitAction)
        {
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index");
            }


            //If user just wants to cancel and return to spreadsheet list
            if (submitAction == "Return")
                return RedirectToAction("SpreadsheetList", new { currentUser });


            //Checks if the spreadsheet already exists
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, currentUser.Username, currentUser.Password, "SpreadsheetList");
            SpreadsheetList list = ReadSpreadsheetList(response);
            if (list == null)
            {
                HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                return RedirectToAction("Index");
            }
            else if (list.Sheets.Contains(spreadsheetName))
            {
                ViewBag.Message = "Error: Spreadsheet with name " + spreadsheetName + " already exists. Please choose another name or delete existing spreadsheet.";
                return View();
            }
            else
            {
                response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, spreadsheetName, null, "CreateSpread");
                //If there is a bug
                if (response.Substring(0, 5).Contains("Error"))
                {
                    HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                    return RedirectToAction("Index");
                }

                //Case when user wants to create another spreadsheet
                if (submitAction == "SaveAndAdd")
                    return RedirectToAction("CreateSpread");
                //Case where the user just wants to save and return to list
                else
                    return RedirectToAction("SpreadsheetList", new { currentUser });
            }
        }

        /// <summary>
        /// Initial Get Call to delete spreadsheet
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteSpread(User currentUser, String spreadsheetName)
        {

            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index");
            }

            //Counts for spreadsheet that has empty character
            if (spreadsheetName == null)
                spreadsheetName = "";

            //Checks if the spreadsheet has already been deleted
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, currentUser.Username, currentUser.Password, "SpreadsheetList");
            SpreadsheetList list = ReadSpreadsheetList(response);
            if (list == null)
            {
                HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                return RedirectToAction("Index");
            }
            else if (!list.Sheets.Contains(spreadsheetName))
            {
                ViewBag.Message = "Notice: Spreadsheet with name " + spreadsheetName + " has already been deleted.";
                return View();
            }


            response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, spreadsheetName, null, "DeleteSpread");
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
            ViewData.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Parses the json string for the spreadsheet list
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Returns null if it fails or returns the spreadsheet list</returns>
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

