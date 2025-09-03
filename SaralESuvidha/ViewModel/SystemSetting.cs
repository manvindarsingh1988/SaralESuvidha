using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SaralESuvidha.ViewModel
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public bool? SystemActive { get; set; }
        public String ElectricityBillInfoUrl { get; set; }
        
        public bool? UPPCLMarginAmountEnabled { get; set; }
        
        public decimal? UPPCLMarginAmount { get; set; }
        public decimal? TestAmount { get; set; }
        public bool? IsDown { get; set; }
        public bool? IsOTSDown { get; set; }
        public bool? RazorTopUp { get; set; }
        public bool? SabPaisaTopUp { get; set; }
        public String IsDownMessage { get; set; }
        
        public string SaveSystemMaintain()
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@IsDownMessage", IsDownMessage);
                    parameters.Add("@IsDown", IsDown);
                    parameters.Add("@IsOTSDown", IsOTSDown);
                    parameters.Add("@RazorTopUp", RazorTopUp);
                    parameters.Add("@SabPaisaTopUp", SabPaisaTopUp);
                    var res = con.Query<string>("usp_SystemSettingUpdate", parameters,
                        commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    result = res;
                }

            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }
            finally
            {
                StaticData.LoadSystemSetting();
            }

            return result;
        }
        
    }

    public class Highlights
    {
        public string GlobalHighlights { get; set; }
    }
}
