using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.ViewModel;

namespace SaralESuvidha.Models
{
    public class UtilityMargin
    {
        [DisplayName("User Id"), Required(ErrorMessage = "Required")]
        public long OrderNo { get; set; }
        public long ID { get; set; }
        public string CreatedByUser { get; set; }
        public string RetailUserid { get; set; }
        public string UtilityType { get; set; }
        
        [DisplayName("Operator Name"), Required(ErrorMessage = "Required")]
        public string OperatorName { get; set; }

        public string Remarks { get; set; }
        public string OperatorCircle { get; set; }

        [DisplayName("Margin Percent"), Required(ErrorMessage = "Required")]
        public float? MarginPercent { get; set; }

        [DisplayName("Max Margin"), Required(ErrorMessage = "Required")]
        public decimal? MaxMargin { get; set; }
        
        [DisplayName("Fix Margin"), Required(ErrorMessage = "Required")]
        public decimal? FixMargin { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal? MarginPercentUpto200 { get; set; }
        public decimal? MaxDailyLimit { get; set; }
        public bool? Active { get; set; }
        public string OperationMessage { get; set; }
        public string FullName { get; set; }
        public string LastUpdateMachine { get; set; }
        public DateTime LastUpdateDate { get; set; }

        public UtilityMargin Save(bool byAdmin=false)
        {
            using (var con = new SqlConnection(StaticData.conString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OrderNo", OrderNo); //reportDate.ToString("MM-dd-yyyy")
                parameters.Add("@OperatorName", OperatorName); 
                parameters.Add("@MarginPercent", MarginPercent);
                parameters.Add("@Maxmargin", MaxMargin);
                parameters.Add("@FixMargin", FixMargin);
                parameters.Add("@MaxDailyLimit", 0);
                parameters.Add("@Remarks", Remarks);
                parameters.Add("@CreatedByUser", CreatedByUser);
                parameters.Add("@LastUpdateDate", DateTime.Now);
                parameters.Add("@LastUpdateMachine", LastUpdateMachine);
                parameters.Add("@MarginPercentUpto200", MarginPercentUpto200);
                parameters.Add("@Active", true);
                
                return byAdmin
                    ? con.QuerySingle<UtilityMargin>("usp_OperatorMarginUpdateRetail", //usp_OperatorMarginUpdate
                        parameters,
                        commandType: System.Data.CommandType.StoredProcedure)
                    : con.QuerySingle<UtilityMargin>("usp_OperatorMarginUpdateRetail",
                        parameters,
                        commandType: System.Data.CommandType.StoredProcedure);
            }
        }


    }
}
