using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.EventResponseType.WalletTransfer
{
    public class Item
    {
        public string transactionId { get; set; }
        public string agencyVan { get; set; }
        public string agentVan { get; set; }
        public string paymentType { get; set; }
        public string transactionType { get; set; }
        public double amount { get; set; }
        public long transactionTime { get; set; }
        public string agentName { get; set; }
        public string referenceId { get; set; }
        public object uniqueId { get; set; }
    }
}
