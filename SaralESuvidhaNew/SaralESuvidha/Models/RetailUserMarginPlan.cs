using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class RetailUserMarginPlan
    {
        public string Id { get; set; }
        public string RetailUserClientId { get; set; }
        public String PlanName { get; set; }
        public string OperatorType { get; set; }
        public string OperatorName { get; set; }
        /// <summary>
        /// F=Flat, P=Percent
        /// </summary>
        public string MarginType { get; set; }
        public decimal? MarginRate { get; set; }
        public decimal? MinMargin { get; set; }
        public decimal? MaxMargin { get; set; }
        public bool? IsSurcharge { get; set; }
        public bool? IsDefault { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreateDate { get; set; }

    }
}
