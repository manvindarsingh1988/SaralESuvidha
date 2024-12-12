using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.OTS
{
    public class CaseInit
    {
        public string AccountId { get; set; }
        public string Discom { get; set; }
        public string TransitionMode { get; set; } = "CM_INITIATE";
        public decimal IpscAmount { get; set; }
        public string SupplyTypeCode { get; set; }
        public decimal TotalOutstandingAmt { get; set; }
        public decimal PrincipalAmt { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal DownPayment { get; set; }
        public decimal ExistingLoad { get; set; }
        public decimal InstallmentAmt { get; set; }
        public int NoOfInstallment { get; set; }
        public string RegistrationOption { get; set; }
        public decimal IpscWaiveOff { get; set; }
        public string ExtraParm1 { get; set; } = "PAYMENT1";
        public string ExtraParm2 { get; set; } = "";
        public string ExtraParm3 { get; set; } = "CM_OTS2024";
    }

    public class CaseInitResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public CaseInitDetails Data { get; set; }
    }
    public class CaseInitDetails
    {
        public string CaseId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ResCode { get; set; }
        public string ResMsg { get; set; }
        public BillDetails BillDetails { get; set; }
        public string Message { get; set; }
    }

    public class BillDetails
    {
        public string AccountInfo { get; set; }
        public decimal? BillAmount { get; set; }
        public string BillDate { get; set; }
        public string BillDueDate { get; set; }
        public string BillId { get; set; }
        public string ConnectionType { get; set; }
        public string ConsumerName { get; set; }
        public string Discom { get; set; }
        public string DivCode { get; set; }
        public string Division { get; set; }
        public string EmailAddress { get; set; }
        public string Error { get; set; }
        public string KNumber { get; set; }
        public decimal? LastPaidAmount { get; set; }
        public string LastPaymentDate { get; set; }
        public string MobileNumber { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public PremiseAddress PremiseAddress { get; set; }
        public string ProjectArea { get; set; }
        public string PurposeOfSupply { get; set; }
        public string SanctionedLoadInBHP { get; set; }
        public string SanctionedLoadInKVA { get; set; }
        public string SanctionedLoadInKW { get; set; }
        public string SdoCode { get; set; }
        public string SubDivision { get; set; }
        public string TdStatus { get; set; }
    }

    public class PremiseAddress
    {
        public string AddressLine1 { get; set; }
        public string City { get; set; }
    }
}
