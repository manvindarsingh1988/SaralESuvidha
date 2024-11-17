using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RTranApiFundTransfer
    {
        public string ApiAccountId { get; set; }

        public decimal? Amount { get; set; }

        public decimal? ClosingBalance { get; set; }

        public string OperationMessage { get; set; }
    }
}
