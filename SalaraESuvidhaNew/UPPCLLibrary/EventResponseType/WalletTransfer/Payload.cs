using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.EventResponseType.WalletTransfer
{
    public class Payload
    {
        public string agencyType { get; set; }
        public double amount { get; set; }
        public string destinationAgentId { get; set; }
        public string referenceId { get; set; }
        public string sourceAgentId { get; set; }
        public string sourceType { get; set; }
        public string transactionId { get; set; }
    }
}
