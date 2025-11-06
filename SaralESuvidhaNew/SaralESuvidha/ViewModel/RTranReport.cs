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

        public string RetailUserId { get; set; }
        public string ClientName { get; set; }
        public int USL { get; set; }
        public string RetailerDetail { get; set; }
        public string ParentName { get; set; }

        public string OperatorName { get; set; }
        public string LiveId { get; set; }
        public string RechargeNumber { get; set; }

        public string RechargeStatus { get; set; }
        public string UPPCL_Status { get; set; }
        public decimal Amount { get; set; }

        public decimal OB { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public decimal Margin { get; set; }

        public decimal CB { get; set; }

       // public string LiveId { get; set; }
        //public string TransactionType { get; set; }

        public DateTime CreateDate { get; set; }
        public string TransactionType { get; set; }
        public string UPPCL_AgentVAN { get; set; }
        public string UPPCL_TransactionId { get; set; }
        public string UPPCL_BillId { get; set; }
        public string UPPCL_PaymentType { get; set; }
        public DateTime UPPCL_TransactionDate { get; set; }
        public decimal UPPCL_Amount { get; set; }
        public decimal UPPCL_BillAmount { get; set; }
        public string UPPCL_ConnectionType { get; set; }

        public string Remarks { get; set; }
        
        //public DateTime RefundRequestDate{ get; set; }
        //public string RefundRequestData { get; set; }
        //public DateTime RefundResponseDate{ get; set; }
        //public string RefundResponse { get; set; }
        //public string RefundStatus { get; set; }
        //public decimal? AccountInfo { get; set; }
        //public decimal? BillAmount { get; set; }
        //public decimal? DDR { get; set; }
        //public int? IsOTS { get; set; }
        public string RefundId { get; set; }
    }
}
