using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RetailClientFundReport
    {
        public string Id { get; set; }

        public string RetailClientName { get; set; }
        
        public string ClientNameBParty { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public decimal ClosingBalance { get; set; }

        public string Remarks { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime TransferDate { get; set; }
    }
}
