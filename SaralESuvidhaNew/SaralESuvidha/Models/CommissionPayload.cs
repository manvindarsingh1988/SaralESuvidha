using System.Text.Json.Serialization;

namespace SaralESuvidha.Models
{
    public class CommissionPayload
    {
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonPropertyName("billId")]
        public string BillId { get; set; }

        [JsonPropertyName("consumerId")]
        public string ConsumerId { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("commissionAmount")]
        public double CommissionAmount { get; set; }

        [JsonPropertyName("areaType")]
        public string AreaType { get; set; }

        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; }

        [JsonPropertyName("agencyId")]
        public string AgencyId { get; set; }

        [JsonPropertyName("agentVan")]
        public string AgentVan { get; set; }

        [JsonPropertyName("empId")]
        public string EmpId { get; set; }

        [JsonPropertyName("connectionType")]
        public string ConnectionType { get; set; }

        [JsonPropertyName("scheme")]
        public string Scheme { get; set; }

        [JsonPropertyName("transactionTime")]
        public string TransactionTime { get; set; }

        
    }
}
