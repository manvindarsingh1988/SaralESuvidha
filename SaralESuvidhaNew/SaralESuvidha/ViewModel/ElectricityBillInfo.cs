using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class ElectricityBillInfo
    {
        public string AccountNumber { get; set; }

        public string OperatorBiller { get; set; }

        public string DetailedOperatorName { get; set; }

        public string CustomerName { get; set; }

        public string Address { get; set; }
        public string BillAmount { get; set; }
        public string PayableAmount { get; set; }

        public string BillDate { get; set; }

        public string EarlyPaymentBillAmount { get; set; }

        public string DueDate { get; set; }

        public string Message { get; set; }
    }
}
