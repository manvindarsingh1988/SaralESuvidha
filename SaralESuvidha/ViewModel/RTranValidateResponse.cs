using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RTranValidateResponse
    {
        public string Id { get; set; }

        public string OperatorName { get; set; }
        public string MobileNumber { get; set; }
        public decimal? Amount { get; set; }
        public string RechargeType { get; set; }
        public string RechargeStatus { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string OperationMessage { get; set; }


    }
}
