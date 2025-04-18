using System;

namespace SaralESuvidha.ViewModel;

public class DailySalesWithCount
{
    public string RetailUserId { get; set; }
    public string RetailerDetail { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CounterLocation { get; set; }
    public int UPTo4KCount { get; set; }
    public decimal UPTo4KTotal { get; set; }
    public int ABOVE4KCount { get; set; }
    public decimal ABOVE4KTotal { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    
}