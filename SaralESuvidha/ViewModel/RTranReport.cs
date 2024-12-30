using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RTranReport
    {
        //public string Id { get; set; }
        public string Rid { get; set; }

        public string Retailer { get; set; }
        public string ClientName { get; set; }

        public string RefundId { get; set; }

        public string OperatorName { get; set; }

        public string RechargeNumber { get; set; }

        public string RechargeStatus { get; set; }
        public decimal OB { get; set; }
        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public decimal Margin { get; set; }

        public decimal CB { get; set; }

        public string LiveId { get; set; }
        public string TransactionType { get; set; }

        public DateTime CreateDate { get; set; }
        public string Remarks { get; set; }
        
        public DateTime RefundRequestDate{ get; set; }
        public string RefundRequestData { get; set; }
        public DateTime RefundResponseDate{ get; set; }
        public string RefundResponse { get; set; }
        public string RefundStatus { get; set; }
        public string PaymentType { get; set; }
        public int? IsOTS { get; set; }
    }
}
