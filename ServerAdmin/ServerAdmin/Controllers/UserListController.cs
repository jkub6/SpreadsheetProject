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
            List<User> users = new List<User>();
            ViewBag.ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            if (ViewBag.ErrorMessage != null)
                return RedirectToAction("Index", "Home");
            else
                return View(users);
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
    }
}