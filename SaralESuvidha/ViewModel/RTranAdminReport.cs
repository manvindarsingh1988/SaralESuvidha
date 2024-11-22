using System;

namespace SaralESuvidha.ViewModel;

public class RTranAdminReport
{
    //public string Id { get; set; }
    public string Rid { get; set; }

    //public string ClientName { get; set; }
    public string RetailUserId { get; set; }
    public string RetailerDetail { get; set; }

    public string ParentName { get; set; }

    public string RefundId { get; set; }

    public string OperatorName { get; set; }
    

    public string RechargeNumber { get; set; }

    public string RechargeStatus { get; set; }
    public decimal OB { get; set; }
    public decimal Amount { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    public decimal Margin { get; set; }

    public decimal CB { get; set; }

    public string LiveId { get; set; }
    public string TransactionType { get; set; }

    //public int? Gateway { get; set; }

    public DateTime CreateDate { get; set; }
    public string PaymentType { get; set; }

    public string Remarks { get; set; }
        
    public DateTime RefundRequestDate{ get; set; }
    public string RefundResponse { get; set; }
    public string RefundStatus { get; set; }
    public string InitialResponseData { get; set; }
}