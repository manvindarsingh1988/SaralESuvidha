using System;

namespace SaralESuvidha.Models;

public class MonitorUserRetailUser
{
    public int Id { get; set; }
    public string MonitorUserId { get; set; }
    public string MonitorName { get; set; }
    public int RetailerUSL { get; set; }
    public string RetailUserId { get; set; }
    public string RetailerName { get; set; }
    public string RetailerMobile { get; set; }
    public string RetailerCity { get; set; }
    public Int64 LastBillDelay { get; set; }
    public Int64 BillTotal { get; set; }
    public int BillCount { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
}

public class MonitorUser
{
    public string Id { get; set; }
    public string LoginName { get; set; }
    public string LoginPassword { get; set; }
    public string MobileNumber { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public bool? Active { get; set; }
}

public class MonitorUserRetailUserWatch
{
    public int Id { get; set; }
    public string MonitorUserId { get; set; }
    public string MonitorName { get; set; }
    public int RetailerUSL { get; set; }
    public string RetailUserId { get; set; }
    public string RetailerName { get; set; }
    public string RetailerMobile { get; set; }
    public string RetailerCity { get; set; }
    public Int64 Delay { get; set; }
    public decimal TAmount { get; set; }
    public int F_2K { get; set; }
    public int F_2K4K { get; set; }
    public int FP_4KA { get; set; }
    public int All { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    
}

