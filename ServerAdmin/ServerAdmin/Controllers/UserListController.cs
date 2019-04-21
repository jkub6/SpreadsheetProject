using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAdmin.Models;
using ServerAdmin.Library;
using System.Net.Sockets;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ServerAdmin.Controllers
{
    public class UserListController : Controller
    {
        private readonly TcpClient tcpClient = new TcpClient();
        public IActionResult Index()
        {
            //Makes sure that the user has not timed out
            User currentUser;
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index", "Home");
            }


            //Connect to server and read out the data
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, currentUser.Username, currentUser.Password, "UserList");
            UserList users = ReadUserList(response);
            if (users != null)
                return View(users);

            //If users is null, means server gave the wrong data for gui to read
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Initial Get Call to Create User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CreateUser(User currentUser)
        {
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (ViewBag.ErrorMessage != null)
                return RedirectToAction("Index", "Home");

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
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, user.Username, user.Password, "CreateUser");
            if (response.Contains("Error"))
            {
                HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));
                return RedirectToAction("SpreadsheetList", "Home");
            }
        }

        /// <summary>
        /// Initial Get Call to Delete User
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteUser(User currentUser)
        {
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (ViewBag.ErrorMessage != null)
                return RedirectToAction("Index", "Home");

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
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, user.Username, user.Password, "DeleteUser");
            if (response.Contains("Error"))
            {
                HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));
                return RedirectToAction("SpreadsheetList", "Home");
            }
        }


        /// <summary>
        /// Parses the json string for the spreadsheet list
        /// </summary>
        /// <param name="json"></param>
        private UserList ReadUserList(String json)
        {
            try
            {
                //Parses the json here
                UserList value = JsonConvert.DeserializeObject<UserList>(json);
                if (value.type == "user")
                    return value;
                else
                    throw new Exception();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}