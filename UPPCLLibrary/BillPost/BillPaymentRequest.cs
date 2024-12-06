using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillPost
{
    public class BillPaymentRequest
    {
        public string agencyType { get; set; }
        public string agentId { get; set; }
        public string amount { get; set; }
        public string billAmount { get; set; }
        public string billId { get; set; }
        public string connectionType { get; set; }
        public string consumerAccountId { get; set; }
        public string consumerName { get; set; }
        public string discom { get; set; }
        public string division { get; set; }
        public string divisionCode { get; set; }
        public string mobile { get; set; }
        public string outstandingAmount { get; set; }
        public string paymentType { get; set; }
        public string referenceTransactionId { get; set; }
        public string sourceType { get; set; }
        public string type { get; set; }
        public string city { get; set; }
        public string param1 { get; set; }
        public string vanNo { get; set; }
        public string walletId { get; set; }

    }
}
