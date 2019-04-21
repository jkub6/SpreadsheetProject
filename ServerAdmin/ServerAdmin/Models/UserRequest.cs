using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAdmin.Models
{
    public class UserRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string spreadsheet { get; set; }

        //Login, CreateUser, DeleteUser, CreateSpread, DeleteSpread
        public string type { get; set; }
    }
}
