using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class PaymentReceipt
    {
        public string Id { get; set; }
        public string ReceiptNumber => Id;
        public string RetailUserId { get; set; }
        public string RefundTransactionId { get; set; }
        public string TelecomOperatorName { get; set; }
        public string ApiOperatorCode { get; set; }
        public string OurApiOperator { get; set; }
        public string OperatorCircle { get; set; }
        public string RechargeMobileNumber { get; set; }
        public string RechargeType { get; set; }
        public string RechargeStatus { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? FinalAmount { get; set; }
        public String Parameter1 { get; set; }
        public String Parameter2 { get; set; }
        public String Parameter3 { get; set; }
        public String Parameter4 { get; set; }
        public decimal? Margin { get; set; }
        public string RequestIp { get; set; }
        public string AmountInWords => StaticData.AmountToInr((double)Amount) + " Only";
        public string RetailerName { get; set; }
        public string LiveId { get; set; }
        public string Extra1 { get; set; }
        public string Extra2 { get; set; }
        public string EndCustomerName { get; set; }

        public string EndCustomerMobileNumber { get; set; }
        public string Remarks { get; set; }
        public string ReceiptMessage { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public long OrderNo { get; set; }

        public string GetOperatorName()
        {
            string result = "";
            result = StaticData.UPPCLOperatorName(TelecomOperatorName);
            return result;
        }

    }
}
