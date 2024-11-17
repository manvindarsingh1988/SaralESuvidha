using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SaralESuvidha.ViewModel
{
    public class RTranReportServerWise
    {
        //public string Id { get; set; }
        public string Rid { get; set; }

        public string ClientId { get; set; }

        public string RefundId { get; set; }

        public string OperatorName { get; set; }
        
        public string OperatorType { get; set; }

        public string OperatorCircle { get; set; }

        public string RechargeNumber { get; set; }

        public string RechargeStatus { get; set; }

        public decimal Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public decimal Margin { get; set; }

        public string LiveId { get; set; }
        public string RefundRemarks { get; set; }
        public string RefundTransactionId { get; set; }
        public string ResponseDetail { get; set; }

        public int? Gateway { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime RefundRequestDate { get; set; }

        public string ProviderApiId { get; set; }
        public DateTime RefundResponseDate { get; set; }
        public Boolean IsRefundProcessed { get; set; }
        
        public string OtherApiId { get; set; }
        public string UPPCL_PaymentType { get; set; }
        public bool UPPCL_TDConsumer { get; set; }
        public string Remarks { get; set; }
        
        
        public RTranReportServerWise LoadForRefundRequest()
        {
            RTranReportServerWise rt = new RTranReportServerWise();
            using (var con = new SqlConnection(StaticData.conString))
            {
                try
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", Rid);
                    rt = con.QuerySingle<RTranReportServerWise>("usp_RTranDetailForDispute", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    rt.Remarks = "Errors: " + ex.Message;
                }
            }

            return rt;
        }
        
    }

}
