using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentActiveInActive
{
    public class User
    {
        public Address address { get; set; }
        public string createdAt { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string id { get; set; }
        public string lastName { get; set; }
        public string mobile { get; set; }
        public string modifiedAt { get; set; }
        public bool @new { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public string userName { get; set; }

        public User()
        {
            address = new Address();
        }
    }
}
