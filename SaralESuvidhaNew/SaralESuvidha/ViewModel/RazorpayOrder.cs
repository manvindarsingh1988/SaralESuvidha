using System;

namespace SaralESuvidha.ViewModel;

public class RazorpayOrder
{
    public string Id { get; set; }
    public string RetailerId { get; set; }
    public string RetailerName { get; set; }
    public string Currency { get; set; }
    public decimal? Amount { get; set; }
    public long? RazorpayAmount { get; set; }
    public long? RazorpayFees { get; set; }
    public long? RazorpayGST { get; set; }
    public long? OurFees { get; set; }
    public string razorpay_payment_id { get; set; }
    public string razorpay_order_id { get; set; }
    public string razorpay_signature { get; set; }
    public string OrderStatus { get; set; }
    public DateTime? OrderStatusDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerMobile { get; set; }
    public string CustomerEmail { get; set; }
    
    public string PaymentType { get; set; }
    public string PaymentNetwork { get; set; }
    public bool? PaymentCaptured { get; set; }
    public DateTime? PaymentCapturedDate { get; set; }
    public bool? IsRefunded { get; set; }
    public DateTime? RefundDate { get; set; }
    public string RefundOrderId { get; set; }
    public bool? IsCredited { get; set; }
    public string CreditTranId { get; set; }
    public DateTime? CreditDate { get; set; }
    public decimal? CreditAmount { get; set; }
    public DateTime? CreateDate { get; set; }
    public string OperationMessage { get; set; }
    
}