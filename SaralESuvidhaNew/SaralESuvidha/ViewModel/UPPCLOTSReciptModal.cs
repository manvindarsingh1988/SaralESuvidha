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
        public string ConsumerName { get; set; }
        public string PurposeOfSupply { get; set; }
        public decimal downPayment { get; set; }
        public string SanctionedLoadInKW { get; set; }
        public string RechargeStatus { get; set; }
        public string ChoosenOption { get; set; }
        public decimal RegistrationAmount_PanjikaranRashi { get; set; }
        public decimal LPSCWaivOff_MafiYogyaAdhikatamByaj { get; set; }
        public decimal LPSC31_Byaj { get; set; }
        public decimal Payment31_Mulbakaya { get; set; }

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
