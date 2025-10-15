using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentCreation
{
    public class Payload
    {
        public string accountNumber { get; set; }
        public Address address { get; set; }
        public string agencyId { get; set; }
        public string agentType { get; set; }
        public List<string> discoms { get; set; }
        public List<string> divisions { get; set; }
        public string email { get; set; }
        public string empId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string mobile { get; set; }
        public string panNumber { get; set; }
        public string userName { get; set; }
        public string van { get; set; }

        public Payload()
        {
            address = new Address();
        }
    }
}
