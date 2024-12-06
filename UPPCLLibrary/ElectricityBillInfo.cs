using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class ElectricityBillInfo
    {
        public string AccountNumber { get; set; }

        public string OperatorBiller { get; set; }

        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string Address { get; set; }
        public string BillPeriod { get; set; }
        public string BillConvnienceFee { get; set; }
        public string BillConvnienceDescription { get; set; }
        public string BillNumber { get; set; }

        public string BillAmount { get; set; }
        public string PayableAmount { get; set; }
        public string EarlyPaymentBillAmount { get; set; }
        public string EarlyBillDate { get; set; }
        public string LateBillAmount { get; set; }

        public string BillDate { get; set; }

        public string DueDate { get; set; }

        public string LastPaymentDate { get; set; }

        public string LastPaymentAmount { get; set; }
        public string DueDateRebate { get; set; }

        public string Message { get; set; }
        public string AdditionalData { get; set; }
    }
}
