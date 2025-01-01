using System;

namespace SaralESuvidha.ViewModel
{
    public class UPPCLOTSReciptModal
    {
        public string ApiOperatorCode { get; set; }
        public string TelecomOperatorName { get; set; }
        public string InfoTable { get; set; }
        public string Para1 { get; set; }
        public string InstallmentTable { get; set; }
        public string RechargeMobileNumber { get; set; }
        public string QrCode { get; set; }

    }

    public class OTSReciptModal
    {
        public string Amount { get; set; }
        public string AccountId { get; set; }
        public int? IsFull { get; set; }
        public string RechargeStatus { get; set; }
        public string LiveId { get; set; }
        public DateTime CreateDate { get; set; }
        public string RetailUserId { get; set; }
        public string RetailerName { get; set; }
        public string ReceiptMessage { get; set; }
    }
}
