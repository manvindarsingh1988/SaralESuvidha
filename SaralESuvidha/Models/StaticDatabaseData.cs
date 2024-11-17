using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SaralESuvidha.ViewModel;
using Microsoft.Extensions.Configuration;

namespace SaralESuvidha.Models
{
    public static class StaticDatabaseData
    {
        
        public static bool apiAccountsLoading = false;

        public static List<ApiConfigOperator> apiConfigOperators;
        public static bool apiConfigOperatorLoading = false;

        public static List<ApiStatusCode> apiApiStatusCodes;
        public static bool apiApiStatusCodeLoading = false;

        public static List<ApiResponseParsing> apiResponseParsings;
        public static bool apiResponseParsingLoading = false;

        public static List<ROfferServerMaster> rOfferServerMasterList = new List<ROfferServerMaster>();
        public static bool rOfferServerMasterListLoading = false;

        public static void LoadAllConfigData()
        {
            LoadApiConfigOperator();
            LoadApiStatusCode();
            LoadApiResponseParsing();
            LoadROfferServerMaster();
        }

        public static bool LoadROfferServerMaster()
        {
            bool result = true;
            try
            {
                rOfferServerMasterList.Clear();
                using (var con = new SqlConnection(StaticData.conString))
                {
                    rOfferServerMasterList = con.Query<ROfferServerMaster>("usp_ROfferServerMasterList",
                        commandType: System.Data.CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                rOfferServerMasterListLoading = false;
            }

            return result;
        }

        public static bool LoadApiConfigOperator()
        {
            bool result = true;
            apiConfigOperatorLoading = true;
            try
            {
                using (var conn = new SqlConnection(StaticData.conString))
                {
                    apiConfigOperators = conn.Query<ApiConfigOperator>("select * from ApiConfigOperator with(nolock) order by ID;").ToList();
                }
            }
            catch (Exception)
            {
                result = false;
                //Utility.WriteException("StaticDatabaseData: LoadApiConfigOperator:" + ex.Message);

            }
            finally
            {
                apiConfigOperatorLoading = false;
            }

            return result;
        }

        public static bool LoadApiStatusCode()
        {
            bool result = true;
            apiApiStatusCodeLoading = true;
            try
            {
                using (var conn = new SqlConnection(StaticData.conString))
                {
                    apiApiStatusCodes = conn.Query<ApiStatusCode>("select * from ApiStatusCode with(nolock) order by ID;").ToList();
                }
            }
            catch (Exception)
            {
                result = false;
                //Utility.WriteException("StaticDatabaseData: ApiStatusCode:" + ex.Message);

            }
            finally
            {
                apiApiStatusCodeLoading = false;
            }

            return result;
        }

        public static bool LoadApiResponseParsing()
        {
            bool result = true;
            apiResponseParsingLoading = true;
            try
            {
                using (var conn = new SqlConnection(StaticData.conString))
                {
                    apiResponseParsings = conn.Query<ApiResponseParsing>("select * from ApiResponseParsing with(nolock) order by ID;").ToList();
                }
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                apiResponseParsingLoading = false;
            }

            return result;
        }

    }
}
