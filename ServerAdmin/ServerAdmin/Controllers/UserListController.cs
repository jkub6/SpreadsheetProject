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
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index", "Home");
            }

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
        public IActionResult CreateUser(User currentUser, String username, String password, String submitAction)
        {
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index", "Home");
            }

            //If user just wants to cancel and return to list of users 
            if (submitAction == "Return")
                return RedirectToAction("Index");

            //Checks if the user already exists
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, currentUser.Username, currentUser.Password, "UserList");
            UserList list = ReadUserList(response);
            if (list == null)
            {
                HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                return RedirectToAction("Index");
            }
            else if (list.users.Contains(username))
            {
                ViewBag.Message = "Error: User with username named " + username + " already exists. Please choose another username or delete existing user.";
                return View();
            }
            else
            {
                response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, username, password, "CreateUser");
                //If there is a bug
                if (response.Length >= 5 && response.Substring(0, 5).Contains("Error"))
                {
                    HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                    return RedirectToAction("Index", "Home");
                }

                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));
                //Case when user wants to create another users
                if (submitAction == "SaveAndAdd")
                    return RedirectToAction("CreateUser");
                //Case where the user just wants to save and return to list of users
                else
                    return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Method that delets the user
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteUser(User currentUser, string username)
        {
            var sessionVar = HttpContext.Session.GetString("CurrentUser");
            if (sessionVar != null)
                currentUser = JsonConvert.DeserializeObject<User>((String)sessionVar);
            else
            {
                HttpContext.Session.SetString("ErrorMessage", "Session Timed Out");
                return RedirectToAction("Index", "Home");
            }

            //Checks if the user has already been deleted 
            String response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, currentUser.Username, currentUser.Password, "UserList");
            UserList list = ReadUserList(response);
            if (list == null)
            {
                HttpContext.Session.SetString("ErrorMessage", "Invalid Data from Server");
                return RedirectToAction("Index");
            }
            else if (!list.users.Contains(username))
            {
                ViewBag.Message = "Notice: User with username named " + username + " has already been deleted.";
                return View();
            }

            response = ServerComm.ConnectToServer(tcpClient, currentUser.IpAddress, username, null, "DeleteUser");
            if (response.Contains("Error"))
            {
                HttpContext.Session.SetString("ErrorMessage", "Authentication Error: Redirecting to Login");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(currentUser));
                //Return to the user list here
                return RedirectToAction("Index");
            }
        }


        /// <summary>
        /// Parses the json string for the user list
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