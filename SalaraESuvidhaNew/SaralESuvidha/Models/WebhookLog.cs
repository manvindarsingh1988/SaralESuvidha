using Dapper;
using Newtonsoft.Json;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SaralESuvidha.Models
{
    public class WebhookLog
    {
        public long Id { get; set; }
        public string RequestIp { get; set; }
        public string RequestData { get; set; }
        public string RequestSignature { get; set; }
        public string EWId { get; set; }
        public string ConnectionType { get; set; }
        public string Scheme { get; set; }
        public DateTime? CreateDate { get; set; }
        public byte? ProcessStatus { get; set; }
        public DateTime? ProcessTime { get; set; }
        public bool IsMatch { get; set; }
        public bool IsAlreadyReceived { get; set; }
        public string ResponseMessage { get; set; }


        public void SaveLog()
        {
            try
            {

                try
                {
                    CommissionPayload commissionPayload = JsonConvert.DeserializeObject<CommissionPayload>(RequestData);
                    EWId = commissionPayload.TransactionId;
                    ConnectionType = commissionPayload.ConnectionType;
                    Scheme = commissionPayload.Scheme;
                    commissionPayload = null;
                }
                catch (Exception ex1)
                {

                }

                using (var con = new SqlConnection(ViewModel.StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RequestIp", RequestIp);
                    parameters.Add("@RequestData", RequestData);
                    parameters.Add("@RequestSignature", RequestSignature);
                    parameters.Add("@EWId", EWId);
                    parameters.Add("@ConnectionType", ConnectionType);
                    parameters.Add("@Scheme", Scheme);
                    parameters.Add("@IsMatch", IsMatch);
                    parameters.Add("@IsAlreadyReceived", IsAlreadyReceived);
                    parameters.Add("@ResponseMessage", ResponseMessage);
                    parameters.Add("@CreateDate", CreateDate);
                    var results = con.Query<ActionResponse>("usp_WebhookLogInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public bool CheckLogInDb()
        {
            bool result = false;
            try
            {
                using (var con = new SqlConnection(ViewModel.StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@EWId", EWId);
                    var results = con.Query<ActionResponse>("usp_WebhookCheck", parameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    if(results != null)
                    {
                        if (results.OperationMessage != "0")
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
