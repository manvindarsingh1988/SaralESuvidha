using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentActiveInActive
{
    public class AgentActiveInActiveResponse
    {
        public string accountNumber { get; set; }
        public string agencyId { get; set; }
        public string agentType { get; set; }
        public double balanceAmount { get; set; }
        public string createdAt { get; set; }
        public List<string> discoms { get; set; }
        public string district { get; set; }
        public List<string> divisions { get; set; }
        public string empId { get; set; }
        public string id { get; set; }
        public string modifiedAt { get; set; }
        public bool @new { get; set; }
        public string panNumber { get; set; }
        public string status { get; set; }
        public User user { get; set; }
        public string van { get; set; }
        public string message { get; set; }

        public AgentActiveInActiveResponse()
        {
            user = new User();
        }
    }
}
