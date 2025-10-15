using System;

namespace SaralESuvidha.ViewModel;

public class RazorpayOrderRetailer
{
    public string Id { get; set; }
    public string RetailerId { get; set; }
    public string RetailerName { get; set; }
    public decimal? Amount { get; set; }
    public decimal? CreditAmount { get; set; }
    public string Currency { get; set; }
    public DateTime? CreateDate { get; set; }
    public string razorpay_payment_id { get; set; }
    public string razorpay_order_id { get; set; }
    public string OrderStatus { get; set; }
    public string CustomerName { get; set; }
    public string CustomerMobile { get; set; }
    public string CustomerEmail { get; set; }
    public bool? IsCredited { get; set; }
    public string CreditTranId { get; set; }
    public DateTime? CreditDate { get; set; }
    public bool? IsRefunded { get; set; }
    public DateTime? RefundDate { get; set; }
    public string RefundOrderId { get; set; }
}