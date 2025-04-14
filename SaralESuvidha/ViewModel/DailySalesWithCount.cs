using System;

namespace SaralESuvidha.ViewModel;

public class DailySalesWithCount
{
    public string RetailUserId { get; set; }
    public string RetailerDetail { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CounterLocation { get; set; }
    public int UPTo2KCount { get; set; }
    public decimal UPTo2KTotal { get; set; }
    public decimal AllUsersAvgUPTo2KCount { get; set; }
    public decimal AllUsersAvgUPTo2KTotal { get; set; }
    public int UPTo2K4KCount { get; set; }
    public decimal UPTo2K4KTotal { get; set; }
    public decimal AllUsersAvg2K4KCount { get; set; }
    public decimal AllUsersAvg2K4KTotal { get; set; }
    public int ABOVE4KCount { get; set; }
    public decimal ABOVE4KTotal { get; set; }
    public decimal AllUsersAvgAbove4KCount { get; set; }
    public decimal AllUsersAvgAbove4KTotal { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    
}