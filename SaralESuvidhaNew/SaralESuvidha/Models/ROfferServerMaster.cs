using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class ROfferServerMaster
    {
        public int Id { get; set; }
        public string ROfferServerName { get; set; }
        public String ServerUrl { get; set; }
        public string OperatorType { get; set; }
        public bool? Active { get; set; }
    }
}
