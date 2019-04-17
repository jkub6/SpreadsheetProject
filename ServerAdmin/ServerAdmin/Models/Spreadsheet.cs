using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAdmin.Models
{
    public class Spreadsheet
    {
        public string Name { get; set; }
        public List<User> UserList { get; set; }
    }
}
