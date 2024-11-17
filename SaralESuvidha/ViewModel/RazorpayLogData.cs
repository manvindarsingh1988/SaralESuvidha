using System;

namespace SaralESuvidha.ViewModel;

public class RazorpayLogData
{
    public long Id { get; set; }
    public string OrderId { get; set; }
    public string RetailerId { get; set; }
    public string CustomerMobile { get; set; }
    public string RazorpayOrderId { get; set; }
    public string LogType { get; set; }
    public string LogData { get; set; }
    public DateTime? CreateDate { get; set; }
}