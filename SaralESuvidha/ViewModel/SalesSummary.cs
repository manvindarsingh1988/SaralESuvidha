using System;

namespace SaralESuvidha.ViewModel
{
    public class SalesSummary
    {
        public string RetailUserId { get; set; }
        public string RetailerName { get; set;}
        public decimal Debit { get; set;}
        public decimal Margin { get; set;}
        public DateTime CreateDate { get; set;}


    }
}
