using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class ROfferServer
    {
        public int Id { get; set; }
        public String OperatorName { get; set; }
        public String OperatorType { get; set; }
        public String DetailedOperatorName { get; set; }
        public string ROfferApiName { get; set; }
        public String ROfferApiOperatorName { get; set; }
        public short? Priority { get; set; }
        public bool? Active { get; set; }

        public bool IsValidKey { get; set; }
    }
}
