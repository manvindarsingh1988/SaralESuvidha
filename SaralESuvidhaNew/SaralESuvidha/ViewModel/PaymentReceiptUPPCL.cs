using System;

namespace SaralESuvidha.ViewModel
{
    public class PaymentReceiptUPPCL
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
        public string UPPCL_ProjectArea { get; set; }
        public bool? UPPCL_TDConsumer { get; set; }
        public string UPPCL_ConnectionType { get; set; }
        public string UPPCL_DivCode { get; set; }
        public string UPPCL_SDOCode { get; set; }
        public decimal? UPPCL_AccountInfo { get; set; }
        public decimal? UPPCL_BillAmount { get; set; }
        public string UPPCL_Division { get; set; }
        public string UPPCL_SubDivision { get; set; }
        public string UPPCL_PurposeOfSupply { get; set; }
        public decimal? UPPCL_SanctionedLoadInKW { get; set; }
        public string UPPCL_BillId { get; set; }
        public string UPPCL_BillDate { get; set; }
        public DateTime? UPPCL_BillPostDate { get; set; }
        public DateTime? UPPCL_BillStatusCheckDate { get; set; }
        public short? UPPCL_BillStatusRetryCount { get; set; }
        public bool? UPPCl_IsForcefullyFailed { get; set; }
        public DateTime? UPPCL_OurForcefullyFailedTime { get; set; }
        public string UPPCL_Discom { get; set; }
        public string UPPCL_ConsumerAddress { get; set; }
        public string QrCode { get; set; }

        public string AgencyName = "Saral ECommerce Pvt Ltd";
        public string AgencyVan = UPPCLLibrary.UPPCLManager.uppclConfig.AgencyVANNo;


        public string GetOperatorName()
        {
            string result = "";
            result = StaticData.UPPCLOperatorName(TelecomOperatorName);
            return result;
        }

        public string GetRTran()
        {
            var tranId = LiveId;
            
            return tranId;
        }

        public string GetRStatus()
        {
            return RechargeStatus;
        }

        public int GetBalanceBillAmount()
        {
            int balanceAmount = Convert.ToInt32(Amount);
            try
            {
                balanceAmount = Convert.ToInt32(Extra2) - Convert.ToInt32(Amount);
            }
            catch (Exception)
            {
                //balanceAmount = 
            }

            return balanceAmount;
        }
        
    }
}
