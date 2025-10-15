using Dapper;
using Org.BouncyCastle.Asn1.Ocsp;
using SaralESuvidha.ViewModel;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json.Serialization;

namespace SaralESuvidha.Models
{
    public class WebhookTransaction
    {
        public long Id { get; set; }
        public string TransactionId { get; set; }
        public string BillId { get; set; }
        public string ConsumerId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? CommissionAmount { get; set; }
        public string AreaType { get; set; }
        public string PaymentType { get; set; }
        public string AgencyId { get; set; }
        public string AgentVAN { get; set; }
        public string RetailerId { get; set; }
        public DateTime? TransactionTime { get; set; }
        public DateTime? CallbackTime { get; set; }
        public byte? ProcessStatus { get; set; }
        public DateTime? ProcessTime { get; set; }
        public string RTranId { get; set; }        
        public string ConnectionType { get; set; }
        public string Scheme { get; set; }
        public string Remarks { get; set; }

        public string Save(CommissionPayload payload)
        {
            string result = "";
            try
            {
                using (var con = new SqlConnection(ViewModel.StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TransactionId", payload.TransactionId);
                    parameters.Add("@BillId", payload.BillId);
                    parameters.Add("@ConsumerId", payload.ConsumerId);
                    parameters.Add("@Amount", payload.Amount);
                    parameters.Add("@CommissionAmount", payload.CommissionAmount);
                    parameters.Add("@AreaType", payload.AreaType);
                    parameters.Add("@PaymentType", payload.PaymentType);
                    parameters.Add("@AgencyId", payload.AgencyId);
                    parameters.Add("@AgentVAN", payload.AgentVan);
                    parameters.Add("@RetailerId", payload.EmpId);
                    parameters.Add("@ConnectionType", payload.ConnectionType);
                    parameters.Add("@Scheme", payload.Scheme);
                    parameters.Add("@TransactionTime", ConvertToDateTime(payload.TransactionTime));
                    parameters.Add("@CallbackTime", DateTime.Now);
                    parameters.Add("@ProcessStatus", 0);
                    var results = con.Query<ActionResponse>("usp_WebhookTransactionInsert", parameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    result = results.OperationMessage;

                    //if (result.ToLower().IndexOf("success") > -1)
                    //{
                        
                    //}
                }


            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }
            return result;
        }

        public static DateTime? ConvertToDateTime(string transactionTime)
        {
            try
            {
                return DateTime.ParseExact(transactionTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null; // Return null if the string format is invalid
            }
        }

    }
}
