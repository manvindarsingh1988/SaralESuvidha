using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentCreation
{
    public class AgentCreationRequest
    {
        public string discoms { get; set; }
        public string divisions { get; set; }
        public string email { get; set; }
        public string empId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string mobile { get; set; }
        public string panNumber { get; set; }
        public string accountNumber { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
    }
}
