using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAdmin.Models
{
    public class UserRequest
    {
        public string IpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        //Login, CreateUser, DeleteUser, CreateSpread, DeleteSpread
        public string Request { get; set; }
    }
}
