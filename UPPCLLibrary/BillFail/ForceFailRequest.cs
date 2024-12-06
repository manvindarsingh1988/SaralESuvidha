using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFail
{
    public class ForceFailRequest
    {
        public string paymentSource { get; set; }
        public string agencyType { get; set; }
        public string amount { get; set; }
        public string billId { get; set; }
        public string consumerId { get; set; }
        public string referenceTransactionId { get; set; }
        public string transactionDate { get; set; }
        public string transactionStatus { get; set; }
        public string vanNo { get; set; }

    }
}
