using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillPost
{
    public class StatusCheckResponse
    {
        public string billAmount { get; set; }
        public string billId { get; set; }
        public string consumerName { get; set; }
        public string consumerNo { get; set; }
        public string discom { get; set; }
        public string division { get; set; }
        public string paymentDate { get; set; }
        public string status { get; set; }
        public string transactionId { get; set; }
        public string vanNo { get; set; }
        public string message { get; set; }
    }
}
