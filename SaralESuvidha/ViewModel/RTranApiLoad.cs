using System;

namespace SaralESuvidha.ViewModel
{
    public class RTranApiLoad
    {
        public string ApiId { get; set; }
        public string RetailClientName { get; set; }
        public string TelecomOperatorName { get; set; }
        public string RechargeMobileNumber { get; set; }
        public decimal Amount { get; set; }
        public string RechargeStatus { get; set; }
        public DateTime CreateDate { get; set; }
    }
}