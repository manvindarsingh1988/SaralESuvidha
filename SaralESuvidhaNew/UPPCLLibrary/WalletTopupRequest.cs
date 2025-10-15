using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class WalletTopupRequest
    {
        public string agencyType { get; set; }
        public string agentVan { get; set; }
        public string agencyVan { get; set; }
        public string amount { get; set; }
        public string referenceId { get; set; }
        public string transactionDate { get; set; } //// Use yyyy-mm-dd format


    }
}
