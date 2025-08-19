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

        [DisplayName("Prepaid Margin Percent"), Required(ErrorMessage = "Required")]
        public decimal? PrepaidMarginPercent { get; set; }

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
            if(MarginPercentUpto200 > 10)
            {
                OperationMessage = "Error: Margin Percent Upto 200 can not be greater than 10%.";
                return this;
            }
            if ( FixMargin > 20)
            {
                OperationMessage = "Error: Fix Margin Amount upto 4000 can not be greater than 20.";
                return this;
            }
            if (MarginPercent > .5)
            {
                OperationMessage = "Error: Margin Percent for Part or more than 4000 full payment can not be greater than 0.5%.";
                return this;
            }
            if (PrepaidMarginPercent != null && PrepaidMarginPercent.GetValueOrDefault() > .4m)
            {
                OperationMessage = "Error: Prepaid Margin Percent can not be greater than 0.4%.";
                return this;
            }
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
                parameters.Add("@PrepaidMarginPercent", PrepaidMarginPercent);
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
