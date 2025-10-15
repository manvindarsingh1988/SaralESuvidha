using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.EventResponseType.WalletTransfer
{
    public class Response
    {
        public string activity { get; set; }
        public double amount { get; set; }
        public string entityId { get; set; }
        public string entityType { get; set; }
        public string referenceId { get; set; }
        public string transactionId { get; set; }
        public string transactionTime { get; set; }
        public string txnId { get; set; }
        public string txnType { get; set; }
        public string vanId { get; set; }
    }
}
