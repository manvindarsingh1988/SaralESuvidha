using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentCreation
{
    public class RetailUser
    {
        public string Id { get; set; }
        public int UserType { get; set; }
        public int USL { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EMail { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PinCode { get; set; }
        public string Country { get; set; }
        public string StateName { get; set; }
        public int Active { get; set; }
        public string AadharNumber { get; set; }
        public string PanNumber { get; set; }
        public string UPPCL_AgentVAN { get; set; }
        public bool UPPCL_Active { get; set; }
        public long DelayTime { get; set; }
        public bool UPPCL_RegInit { get; set; }
        public DateTime UPPCL_RegInitTime { get; set; }
        public DateTime UPPCL_EventTime { get; set; }
        public string UPPCL_EventId { get; set; }
        public decimal UPPCL_Balance { get; set; }
        public DateTime UPPCL_BalanceTime { get; set; }
        public string UPPCL_Status { get; set; }
        public string Discom { get; set; }


    }
}
