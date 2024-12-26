using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class PaymentDetailsResponse
    {
        public string KNumber { get; set; }
        public string ConsumerName { get; set; }
        public string AccountInfo { get; set; }
        public PremiseAddress PremiseAddress { get; set; }
        public string SubDivision { get; set; }
        public string Division { get; set; }
        public string PurposeOfSupply { get; set; }
        public string SanctionedLoadInBHP { get; set; }
        public string SanctionedLoadInKVA { get; set; }
        public string SanctionedLoadInKW { get; set; }
        public string TDStatus { get; set; }
        public string Discom { get; set; }
        public string MobileNumber { get; set; }
        public string EmailAddress { get; set; }
        public string BillID { get; set; }
        public string BillAmount { get; set; }
        public string BillDueDate { get; set; }
        public object LastPaidAmount { get; set; }
        public string LastPaymentDate { get; set; }
        public string BillDate { get; set; }
        public string ConnectionType { get; set; }
        public string DivCode { get; set; }
        public string SDOCode { get; set; }
        
        public string ProjectArea { get; set; }
        public object Param1 { get; set; }
        public object Param2 { get; set; }
        public string LifelineAct { get; set; }

        public PaymentDetailsResponse()
        {
            PremiseAddress = new PremiseAddress();
        }
    }
}
