using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RetailUserBalanceResponse
    {
        public int Order { get; set; }
        public decimal Balance { get; set; }
        public string OperationMessage { get; set; }
    }
}
