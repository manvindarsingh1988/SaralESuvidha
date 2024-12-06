using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillPost
{
    public class BillPostResponse
    {
        public string agencyType { get; set; }
        public string agentId { get; set; }
        public decimal amount { get; set; }
        public string billId { get; set; }
        public string connectionType { get; set; }
        public string consumerAccountId { get; set; }
        public string consumerName { get; set; }
        public string discom { get; set; }
        public string division { get; set; }
        public string divisionCode { get; set; }
        public string externalTransactionId { get; set; }
        public string mobile { get; set; }
        public string referenceTransactionId { get; set; }
        public string sourceType { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string vanNo { get; set; }
        public string walletId { get; set; }
        public string message { get; set; }
    }
}
