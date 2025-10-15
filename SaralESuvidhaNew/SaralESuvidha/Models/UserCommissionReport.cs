using System;

namespace SaralESuvidha.Models
{
    public class UserCommissionReport
    {
        public string Id { get; set; }
        public int OrderNo { get; set; }
        public string RetailerName { get; set; }
        public string RetailerType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Commission { get; set; }
        public decimal ToDateBalance { get; set; }
        public string ParentId { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        
    }
}