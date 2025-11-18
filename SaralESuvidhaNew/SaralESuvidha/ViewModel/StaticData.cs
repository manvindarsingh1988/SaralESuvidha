using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Asn1.Ocsp;
using SaralESuvidha.Controllers;
using SaralESuvidha.Models;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UPPCLLibrary;
using UPPCLLibrary.AgentActiveInActive;
using RTran = SaralESuvidha.Models.RTran;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SaralESuvidha.ViewModel
{
    public static class StaticData
    {
        public static string conString = Startup.ConnectionString;
        public static string logFileName = DateTime.Now.ToString("yy-MM-dd") + "_ajax.txt";
        public static string loginSource = "web";
        public static string rofferMobileUrl = "";
        public static string rofferDthCustInfoUrl = "";
        public static string webhookSharedSecret = "";
        public static string rzp_ApiKey = "rzp_live_gMiAscmFhv8wrv";
        public static string rzp_ApiSecret = "jtX8wTfpArPLse0PerpTkthO";

        const int ColumnBase = 26;
        const int DigitMax = 7; // ceil(log26(Int32.Max))
        const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static RetailUser retailUser = new RetailUser();
        public static SystemSetting systemSetting = new SystemSetting();

        public static string ComputeHmacSha256V0(string message, string secret)
        {
            var encoding = Encoding.UTF8;
            using var hmac = new HMACSHA256(encoding.GetBytes(secret));
            //var hash = hmac.ComputeHash(encoding.GetBytes(message));
            var hash = hmac.ComputeHash(encoding.GetBytes(JsonConvert.SerializeObject(message)));
            return Convert.ToHexString(hash).ToLower();
        }

        public static string ComputeHmacSha256(string message, string secret)
        {
            var encoding = Encoding.UTF8;

            using var hmac = new HMACSHA256(encoding.GetBytes(secret));

            var hash = hmac.ComputeHash(encoding.GetBytes(message));

            return Convert.ToHexString(hash).ToLower();
        }

        public static string ComputeHmacSha256_V1(string message, string secret)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(message);
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(secretBytes);
            byte[] dataBytes = Encoding.UTF8.GetBytes(json);
            byte[] hash = hmac.ComputeHash(dataBytes);
            string computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return computedSignature;
        }



        public static string IndexToColumn(int index)
        {
            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            if (index <= ColumnBase)
                return Digits[index - 1].ToString();

            var sb = new StringBuilder().Append(' ', DigitMax);
            var current = index;
            var offset = DigitMax;
            while (current > 0)
            {
                sb[--offset] = Digits[--current % ColumnBase];
                current /= ColumnBase;
            }
            return sb.ToString(offset, DigitMax - offset);
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static string GetApiNameFromCallback(string callBackUrl)
        {
            return ConvertHexToString(GetRegExFirstMatch(callBackUrl, "a=(.*?)[&|?]"));
        }

        public static bool CustomValidationByExpression(string dataToValidate, string validationExpression)
        {
            try
            {
                Regex pattern = new Regex(validationExpression);
                return pattern.IsMatch(dataToValidate);
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        internal static int StringToInt(string StringValue)
        {
            int result = 0;

            try
            {
                result = Convert.ToInt32(StringValue);
            }
            catch (Exception)
            {
            }
            finally
            {
            }

            return result;
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        internal static decimal StringToDecimal(string StringValue)
        {
            decimal result = 0;

            try
            {
                result = Convert.ToDecimal(StringValue);
            }
            catch (Exception)
            {
            }
            finally
            {
            }

            return result;
        }

        [System.Reflection.ObfuscationAttribute(Feature = "renaming")]
        public static string GetSQLDate()
        {
            string result = string.Empty;
            try
            {
                result = DateTime.UtcNow.AddHours(5.5).ToString("yyyy-MMM-dd HH:mm:ss");
            }
            catch (Exception)
            {
            }

            return result;
        }

        internal static String GetRegExFirstMatch(string inputString, string regexValidation)
        {
            string result = String.Empty;

            try
            {
                Match match = Regex.Match(inputString, regexValidation, RegexOptions.None);
                if (match.Success)
                {
                    result = match.Groups[1].Value;
                }
            }
            catch (Exception)
            {

            }


            return result;
        }

        public static string RandomNumberString()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[4];
            provider.GetBytes(byteArray);

            //convert 4 bytes to an integer
            var randomInteger = BitConverter.ToUInt32(byteArray, 0);

            var byteArray2 = new byte[8];
            provider.GetBytes(byteArray2);

            //convert 8 bytes to a double
            string randomDouble = BitConverter.ToDouble(byteArray2, 0).ToString();
            randomDouble = randomDouble.Replace(",", "");
            randomDouble = randomDouble.ToString().Length > 15
                ? randomDouble.ToString().Substring(0, 14)
                : randomDouble;
            return randomDouble.Replace("-","").Replace(".","");
        }

        public static string Random6DigitPINString()
        {
            return RandomNumberString().Substring(0, 6);
        }

        public static string EncodePIN(string pin)
        {
            return StaticData.EncodeBase64(System.Text.Encoding.Unicode, PSFTCrypto.Encrypt(pin));
        }

        public static string DecodePIN(string enc)
        {
            return PSFTCrypto.Decrypt(StaticData.DecodeBase64(System.Text.Encoding.Unicode, enc));
        }

        public static string CountriesListJson()
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    List<Country> allCountries = con.Query<Country>("select Id, [Name] as CountryName from Countries with(nolock) order by Id;").ToList();
                    //result = con.Query<string>("select Id, [Name] as CountryName from Countries with(nolock) order by Id for Json PATH, INCLUDE_NULL_VALUES;").ToList()[0];
                    result = JsonConvert.SerializeObject(allCountries);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static void LoadSystemSetting()
        {
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    systemSetting = con.QuerySingleOrDefault<SystemSetting>("usp_SystemSetting", commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
            }
        }

        public static bool CheckTopupServiceIsDown(string type)
        {
            LoadSystemSetting();
            if (type == "Razor" && !systemSetting.RazorTopUp.GetValueOrDefault())
            {
                return false;
            }
            if (type == "SabPaisa" && !systemSetting.SabPaisaTopUp.GetValueOrDefault())
            {
                return false;
            }
            return true;
        }

        public static string OperatorListJson()
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    List<OperatorList> allOperator = con.Query<OperatorList>("usp_OperatorListForMargin", commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = JsonConvert.SerializeObject(allOperator);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static string StateListJson(int countryId)
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@CntId", countryId);
                    List<Region> allStates = con.Query<Region>("usp_RegionsSelectByCountry", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = JsonConvert.SerializeObject(allStates);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static string CitiesListJson(int regionId)
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RegionId", regionId);
                    List<City> allCities = con.Query<City>("usp_CitiesSelectByRegion", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = JsonConvert.SerializeObject(allCities);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static List<KYCToken> GetKYCTokens()
        {
            List<KYCToken> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<KYCToken>("usp_GetKYCTokens", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static bool InsertOrUpdateKYCTokens(string token, DateTime createdOn)
        {
            var result = true;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Token", token);
                    parameters.Add("@CreatedOn", createdOn);
                    con.Execute("usp_InsertOrUpdateKYCToken", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        

        public static string GeneratePassword(int length)
        {
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return GetRandomString(length, alphanumericCharacters);
        }

        public static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            try
            {
                if (length < 0)
                    throw new ArgumentException("length must not be negative", "length");
                if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
                    throw new ArgumentException("length is too big", "length");
                if (characterSet == null)
                    throw new ArgumentNullException("characterSet");
                var characterArray = characterSet.Distinct().ToArray();
                if (characterArray.Length == 0)
                    throw new ArgumentException("characterSet must not be empty", "characterSet");

                var bytes = new byte[length * 8];
                new RNGCryptoServiceProvider().GetBytes(bytes);
                var result = new char[length];
                for (int i = 0; i < length; i++)
                {
                    ulong value = BitConverter.ToUInt64(bytes, i * 8);
                    result[i] = characterArray[value % (uint) characterArray.Length];
                }

                return new string(result);
            }
            catch(Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }

        public static string AmountToInr(double? numbers, Boolean paisaconversion = false)
        {
            var pointindex = numbers.ToString().IndexOf(".");
            var paisaamt = 0;
            if (paisaconversion)
            {
                if (pointindex > 0)
                    paisaamt = Convert.ToInt32(numbers.ToString().Substring(pointindex + 1, 2));
            }

            int number = Convert.ToInt32(numbers);

            if (number == 0) return "Zero";
            if (number == -2147483648) return "Minus Two Hundred and Fourteen Crore Seventy Four Lakh Eighty Three Thousand Six Hundred and Forty Eight";
            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }
            string[] words0 = { "", "One ", "Two ", "Three ", "Four ", "Five ", "Six ", "Seven ", "Eight ", "Nine " };
            string[] words1 = { "Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ", "Fifteen ", "Sixteen ", "Seventeen ", "Eighteen ", "Nineteen " };
            string[] words2 = { "Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ", "Seventy ", "Eighty ", "Ninety " };
            string[] words3 = { "Thousand ", "Lakh ", "Crore " };
            num[0] = number % 1000; // units
            num[1] = number / 1000;
            num[2] = number / 100000;
            num[1] = num[1] - 100 * num[2]; // thousands
            num[3] = number / 10000000; // crores
            num[2] = num[2] - 100 * num[3]; // lakhs
            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;
                u = num[i] % 10; // ones
                t = num[i] / 10;
                h = num[i] / 100; // hundreds
                t = t - 10 * h; // tens
                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i == 0) sb.Append(""); //and 
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }

            if (paisaamt == 0 && paisaconversion == false)
            {
                sb.Append(""); //Rs Only
            }
            else if (paisaamt > 0)
            {
                var paisatext = AmountToInr(paisaamt, true);
                sb.AppendFormat("Rupees {0} Paise Only", paisatext);
            }
            return sb.ToString().TrimEnd();
        }

        public static bool IsNumber(string NumberToCheck)
        {
            bool result = false;
            Regex regex = new Regex(@"^[0-9]+$"); //^\d$
            try
            {
                result = regex.IsMatch(NumberToCheck);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static bool IsDecimal(string input)
        {
            Decimal dummy;
            return Decimal.TryParse(input, out dummy);
        }
        
        public static string EncodeBase64(this System.Text.Encoding encoding, string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }

        public static string DecodeBase64(this System.Text.Encoding encoding, string encodedText)
        {
            if (encodedText == null)
            {
                return null;
            }

            byte[] textAsBytes = System.Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }

        public static string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue != null && HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }

        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }
        public static string ReadURL(String ReadURL)
        {
            string result = string.Empty;
            try
            {
                WebClient client = new WebClient();
                String baseurl = ReadURL;
                Stream data = client.OpenRead(baseurl);
                StreamReader reader = new StreamReader(data);
                result = reader.ReadToEnd();
                data.Close();
                reader.Close();
            }
            catch (Exception)
            {
            }

            return result;
        }

        public static string UPPCLOperatorName(string operatorName)
        {
            string result = operatorName;
            if (operatorName == "UPPCLU_PUVVNL")
            {
                result = "PUVVNL";
            }else if (operatorName == "UPPCLU_PVVNL")
            {
                result = "PVVNL";
            }else if (operatorName == "UPPCLU_MVVNL")
            {
                result = "MVVNL";
            }else if (operatorName == "UPPCLU_DVVNL")
            {
                result = "DVVNL";
            }
            return result;
        }
        
        public static string ADNOperatorName(string operatorName)
        {
            string result = operatorName;
            if (operatorName == "PUVVNL")
            {
                result = "UPPCLU_PUVNL";
            }else if (operatorName == "PVVNL")
            {
                result = "UPPCLU_PVVNL";
            }else if (operatorName == "MVVNL")
            {
                result = "UPPCLU_MVVNL";
            }else if (operatorName == "DVVNL")
            {
                result = "UPPCLU_DVVNL";
            }
            return result;
        }


        public static string UserListByType(string userType, string fileName, string filePath, bool export = false)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserType", userType);
                    List<RetailUserGrid> allRecords = con.Query<RetailUserGrid>("usp_RetailUserListByType", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (export)
                    {
                        if (allRecords.Count > 0)
                        {
                            using (DataTable dt = allRecords.ToDataTable())
                            {
                                dt.Columns.Remove("UserTypeString");
                                dt.Columns.Remove("Balance");
                                dt.Columns.Remove("MarginType");
                                dt.Columns.Remove("Parent");
                                result = DataTableToExcelEP(dt, fileName, filePath);
                            }
                        }
                        else
                        {
                            result = "Errors: No data found for specified search criteria.";
                        }
                    }
                    else
                    {
                        var aaData = new { data = allRecords };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {

            }

            return result;
        }

        public static string OperatorUtilityList(string utilitytype, bool forGrid = false)
        {
            string result = string.Empty;
            try
            {
                utilitytype = ConvertHexToString(utilitytype);
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@OperatorType", utilitytype);
                    List<OperatorMasterDdl> allRecords = con.Query<OperatorMasterDdl>("usp_OperatorListByType", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (forGrid)
                    {
                        var aaData = new { data = allRecords };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(allRecords);
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {

            }

            return result;
        }

        public static (RetailUserGrid, int?) ValidateRetailUserLogin(string mobileNumber, string password, string f="", string d="")
        {
            RetailUserGrid retailUser = new RetailUserGrid();
            int? inactive;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@MobileNumber", mobileNumber);
                    queryParameters.Add("@Password", password);
                    if (!string.IsNullOrEmpty(f))
                    {
                        queryParameters.Add("@FToken", f);
                    }
                    if (!string.IsNullOrEmpty(d))
                    {
                        queryParameters.Add("@Did", d);
                    }
                    queryParameters.Add("@OutResult", dbType: DbType.Int32, direction: ParameterDirection.Output, size: 5215585);
                    retailUser = con.QuerySingleOrDefault<RetailUserGrid>("usp_RetailClient_ValidateLogin", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    inactive = queryParameters.Get<int?>("@OutResult");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return (retailUser, inactive);
        }

        public static AppUserLogin ValidateOfficeLogin(string mobileNumber, string password)
        {
            AppUserLogin officeUser = new AppUserLogin();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@MobileNumber", mobileNumber);
                    queryParameters.Add("@Password", password);
                    officeUser = con.QuerySingleOrDefault<AppUserLogin>("usp_Office_ValidateLogin", queryParameters, commandType: System.Data.CommandType.StoredProcedure);

                }
            }
            catch (Exception)
            {

            }
            return officeUser;
        }
        
        public static string RTranApiLoadByDate(DateTime reportDateFrom, DateTime reportDateTo, string filePath)
        {
            var aaIData = new RTranApiLoad();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    List<RTranApiLoad> allDailyRecharge = con.Query<RTranApiLoad>("usp_RTranForApiLoad", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "ApiLoad", filePath);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: " + ex.Message;
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string SalesReportAllByDate(DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@OrderNo", 0);
                    parameters.Add("@TranDate", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateEnd", reportDateTo.ToString("MM-dd-yyyy"));
                    List<SalesSummary> allDailyRecharge = con.Query<SalesSummary>("usp_SalesSummaryByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport == 1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailySales", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        public static string SalesReportWithCountAllByDate(DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@StartDate", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@EndDate", reportDateTo.ToString("MM-dd-yyyy"));
                    List<DailySalesWithCount> allDailyRecharge = con.Query<DailySalesWithCount>("usp_GetRechargeMetricsByRetailUserWithAverages", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport == 1)
                    {
                        
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailySalesWithCount", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string DownlineSalesReportRetailClientByDate(string retailClientId, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDate", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateEnd", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@RetailUserId", retailClientId);
                    List<SalesSummary> allDailyRecharge = con.Query<SalesSummary>("usp_SalesSummaryDownlineByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport == 1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailySales", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string SalesReportRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDate", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<SalesSummary> allDailyRecharge = con.Query<SalesSummary>("usp_SalesSummaryByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (excelExport == 1)
                        {
                            result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailySales", filePath);
                        }
                        else
                        {
                            var aaData = new { data = allDailyRecharge };
                            result = JsonConvert.SerializeObject(aaData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string TopupReportRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDate", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateEnd", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        if (retailClientOrderNo == 999999999)
                        {
                            List<RazorpayOrder> allDailyRecharge = con.Query<RazorpayOrder>("usp_TopupSummaryByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                            if (excelExport == 1)
                            {
                                result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "AccountTopup", filePath);
                            }
                            else
                            {
                                var aaData = new { data = allDailyRecharge };
                                result = JsonConvert.SerializeObject(aaData);
                            }
                        }
                        else
                        {
                            List<RazorpayOrderRetailer> allDailyRecharge = con.Query<RazorpayOrderRetailer>("usp_TopupSummaryByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                            if (excelExport == 1)
                            {
                                result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "AccountTopup", filePath);
                            }
                            else
                            {
                                var aaData = new { data = allDailyRecharge };
                                result = JsonConvert.SerializeObject(aaData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string RechargeReportRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > -1)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_RechargeReportRetailClientByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (excelExport==1)
                        {
                            result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyRecharge_U-Wallet", filePath);
                        }
                        else
                        {
                            var aaData = new { data = allDailyRecharge };
                            result = JsonConvert.SerializeObject(aaData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string RechargeReportSWalletRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > -1)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_RechargeReportSWalletRetailClientByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (excelExport==1)
                        {
                            result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyRecharge_S-Wallet", filePath);
                        }
                        else
                        {
                            var aaData = new { data = allDailyRecharge };
                            result = JsonConvert.SerializeObject(aaData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string RechargeReportUPPCLByDate(DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new UPPCLReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    List<UPPCLReport> allDailyRecharge = con.Query<UPPCLReport>("usp_RechargeReportUPPCLByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport==1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "UPPCLDailyRecharge", filePath);
                        result = result.Replace("Download excel file", "Download excel file internal");
                        result += "<br/>";
                        result += DataTableToExcelEP(allDailyRecharge.ToDataTable(), "UPPCLDailyRecharge", filePath, new string[2] { "RId", "Payment_Type" });
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Account_Number = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string DataTableToExcelEP(DataTable dataTableExcel, string fileName, string filePath, string[] removeColumns)
        {
            foreach (var column in removeColumns)
            {
                dataTableExcel.Columns.Remove(column);
            }
            return DataTableToExcelEP(dataTableExcel, fileName, filePath);
        }

        public static string AllUserReportResult(int excelExport, string filePath = "")
        {
            var aaIData = new UPPCLReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    List<AllUserWithBalance> allDailyRecharge = con.Query<AllUserWithBalance>("usp_RetailUserListWithBalance", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport==1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "AllUsersReport", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Account_Number = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string AllUserReportResultByUserAndDate(int excelExport, DateTime date, int id, string filePath = "")
        {
            var aaIData = new UPPCLReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@date", date);
                    if(id > 0)
                    {
                        parameters.Add("@Id", id);
                    }  
                    else
                    {
                        parameters.Add("@Id", null);
                    }
                    List<AllUserWithBalance> allDailyRecharge = con.Query<AllUserWithBalance>("usp_RetailUserListWithBalanceByUserAndDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport == 1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "AllUsersReportByUserAndDate", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Account_Number = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string AllPnLReportResultByUserAndDate(int excelExport, DateTime dateFrom, DateTime dateTo, int id, string filePath = "", string userId = "")
        {
            var aaIData = new UPPCLReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@dateFrom", dateFrom);
                    parameters.Add("@dateTo", dateTo);
                    if (id > 0)
                    {
                        parameters.Add("@Id", id);
                    }
                    else
                    {
                        parameters.Add("@Id", null);
                    }
                    if(userId != "")
                    {
                        parameters.Add("@userId", userId);
                    }
                    List<AllUserWithBalance> allDailyRecharge = new List<AllUserWithBalance>();
                    if(userId != "")
                    {
                        allDailyRecharge = con.Query<AllUserWithBalance>("usp_RetailUserListWithUPPCLCommissionByUserId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    }
                    else
                    {
                        allDailyRecharge = con.Query<AllUserWithBalance>("usp_RetailUserListWithUPPCLCommission", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    }
                    
                    if (excelExport == 1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "AllUsersReportByUserAndDate", filePath);
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Account_Number = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        internal static string AddSalary(string retailerId, int salAmount)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(retailerId)) { throw new Exception(); }

            try
            { 
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@SalaryAmount", salAmount);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_updateSalaryAmount", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Errors: Exception in updating salary.";// + ex.Message;
            }
            finally
            {

            }

            return result;        
        }


        public static string RechargeReportDistributorByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_RechargeReportDistributorByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (excelExport==1)
                        {
                            result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyRechargeDist", filePath);
                        }
                        else
                        {
                            var aaData = new { data = allDailyRecharge };
                            result = JsonConvert.SerializeObject(aaData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string RechargeSummaryReportRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_RechargeSummaryReportRetailClientByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        if (excelExport == 1)
                        {
                            result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyRecharge", filePath);
                        }
                        else
                        {
                            var aaData = new { data = allDailyRecharge };
                            result = JsonConvert.SerializeObject(aaData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string RechargeReportAllRetailClientByDate(DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    List<RTranAdminReport> allDailyRecharge = con.Query<RTranAdminReport>("usp_RechargeReportAllRetailClientAdminByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport==1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyAllRecharge_S-WALLET", filePath);
                        
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string StatementReportDistributorRetailClientByDate(DateTime reportDateFrom, DateTime reportDateTo, int distributorOrderNo, int excelExport, string filePath = "")
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@OrderNo", distributorOrderNo);
                    List<RTranAdminReport> allDailyRecharge = con.Query<RTranAdminReport>("usp_RetailerStatementReportDistributorByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport==1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyAllStatement", filePath);
                        
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static List<RTranReport> RechargeReportListRetailClientByDate(int retailClientOrderNo, DateTime reportDateFrom, DateTime reportDateTo)
        {
            List<RTranReport> allDailyRecharge = new List<RTranReport>();
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        allDailyRecharge = con.Query<RTranReport>("usp_RechargeReportRetailClientByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    }
                }
            }
            catch (Exception)
            {
                
            }
            return allDailyRecharge;
        }

        public static string SearchNumberByDateByClient(string numberRecharge, DateTime reportDateFrom, int retailClientOrderNo)
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                if (retailClientOrderNo > 0)
                {
                    using (var con = new SqlConnection(StaticData.conString))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@RechargeNumber", numberRecharge);
                        parameters.Add("@TranDate", reportDateFrom.ToString("MM-dd-yyyy"));
                        parameters.Add("@OrderNo", retailClientOrderNo);
                        List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_SearchNumberByDateByClient", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string SearchNumberByDate(string numberRecharge, DateTime reportDateFrom, DateTime reportDateTo)
        {
            var aaIData = new RTranReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RechargeNumber", numberRecharge);
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    
                    List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_SearchNumberByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = allDailyRecharge };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string MarginSheetListByRetailer(string userId)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserId", userId);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetList", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string MarginSheetDownlineByRetailer(string retailClientId)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", retailClientId);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetDownlineByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string MarginSheetDownlineByRetailerWhiteLabel(string retailClientId)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", retailClientId);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetDownlineByClientIdWhiteLabel", parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string MarginSheetByRetailer(string retailClientId)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", retailClientId);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string SystemSettingList()
        {
            var aaIData = new SystemSetting();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", 1);
                    List<SystemSetting> systemSettings = con.Query<SystemSetting>("usp_SystemSettingSelect", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = systemSettings };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.ElectricityBillInfoUrl = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string MarginSheetByRetailerOrderNo(int retailClientOrderNo)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserOrderNo", retailClientOrderNo);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetByOrderNo", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string MarginSheetByRetailerOrderNoParentValidation(int retailClientOrderNo, string parentId)
        {
            var aaIData = new MarginSheet();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserOrderNo", retailClientOrderNo);
                    parameters.Add("@ParentId", parentId);
                    List<MarginSheet> marginSheets = con.Query<MarginSheet>("usp_RetailClientMarginSheetByOrderNoParentValidation", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = marginSheets };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.OperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string MarginReportByRetailer(string retailClientId, DateTime dateFrom, DateTime dateTo, bool IsMaster=false)
        {
            var aaIData = new MarginReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", retailClientId);
                    parameters.Add("@DateFrom", dateFrom);
                    parameters.Add("@DateTo", dateTo);
                    if (IsMaster)
                    {
                        List<MarginReport> marginReport = con.Query<MarginReport>("usp_MasterRetailClientMarginReportByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        var aaData = new { data = marginReport };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else
                    {
                        List<MarginReport> marginReport = con.Query<MarginReport>("usp_RetailClientMarginReportByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                        var aaData = new { data = marginReport };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                aaIData.TelecomOperatorName = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string FunTransferBetweenPeriod(DateTime reportDateFrom, DateTime reportDateTo, string retailClientId)
        {
            var aaIData = new RetailClientFundReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@RetailUserId", retailClientId);
                    List<RetailClientFundReport> allDailyRecharge = con.Query<RetailClientFundReport>("usp_RetailClientFundTransferBetweenPeriodByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = allDailyRecharge };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string RefundBetweenPeriod(DateTime reportDateFrom, DateTime reportDateTo, string retailClientId)
        {
            var aaIData = new RetailClientFundReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@RetailUserId", retailClientId);
                    List<RTranReport> allDailyRecharge = con.Query<RTranReport>("usp_RetailClientRefundBetweenPeriodByClientId", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = allDailyRecharge };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string FundTransferBetweenPeriodOffice(DateTime reportDateFrom, DateTime reportDateTo, int retailOrderNo, string bySource="All", string export = "0", string fileName="", string filePath="")
        {
            var aaIData = new RetailClientFundReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@OrderNo", retailOrderNo);
                    parameters.Add("@BySource", bySource);
                    List<RetailClientFundReport> allDailyRecharge = con.Query<RetailClientFundReport>("usp_OfficeRetailClientFundTransferBetweenPeriodByClientOrderNo", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (export == "0")
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else if(export == "1")
                    {
                        if (allDailyRecharge.Count > 0)
                        {
                            using (DataTable dt = allDailyRecharge.ToDataTable())
                            {
                                dt.Columns.Remove("Id");
                                dt.Columns.Remove("TransferDate");
                                result = DataTableToExcelEP(dt, fileName, filePath);
                            }
                        }
                        else
                        {
                            result = "Errors: No data found for specified search criteria.";
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string CommissionBetweenPeriodOffice(DateTime reportDateFrom, DateTime reportDateTo, int userType, string export = "0", string fileName="", string filePath="")
        {
            var aaIData = new RetailClientFundReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@UserType", userType);
                    List<UserCommissionReport> allDailyRecharge = con.Query<UserCommissionReport>("usp_OfficeRetailClientCommissionBetweenPeriodByClientType", parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600).ToList();
                    if (export == "0")
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else if(export == "1")
                    {
                        if (allDailyRecharge.Count > 0)
                        {
                            using (DataTable dt = allDailyRecharge.ToDataTable())
                            {
                                result = DataTableToExcelEP(dt, fileName, filePath);
                            }
                        }
                        else
                        {
                            result = "Errors: No data found for specified search criteria.";
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static string CommissionBetweenPeriodWhiteLabel(DateTime reportDateFrom, DateTime reportDateTo, string whiteLabelId, string export = "0", string fileName="", string filePath="")
        {
            var aaIData = new RetailClientFundReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    parameters.Add("@RetailUserId", whiteLabelId);
                    List<UserCommissionReport> allDailyRecharge = con.Query<UserCommissionReport>("usp_OfficeRetailClientCommissionBetweenPeriodByWhiteLabel", parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600).ToList();
                    if (export == "0")
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else if(export == "1")
                    {
                        if (allDailyRecharge.Count > 0)
                        {
                            using (DataTable dt = allDailyRecharge.ToDataTable())
                            {
                                result = DataTableToExcelEP(dt, fileName, filePath);
                            }
                        }
                        else
                        {
                            result = "Errors: No data found for specified search criteria.";
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        public static string RechargePendingReport()
        {
            var aaIData = new RTranReportServerWise();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    List<RTranReportServerWise> allDailyRecharge = con.Query<RTranReportServerWise>("usp_RechargePendingReport", commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = allDailyRecharge };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                aaIData.Remarks = ex.Message;
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }
        
        public static RetailUser RetailUserOrderNoNameMobile(string retailerId)
        {
            RetailUser result = new RetailUser();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@RetailUserId", retailerId);
                    result = con.QuerySingleOrDefault<RetailUser>("usp_RetailUserOrderNoNameMobile", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    result.RequestDomain = "success";
                }
            }
            catch (Exception ex)
            {
                result.RequestDomain = ex.Message;
            }
            return result;
        }

        public static string UpdateUPPCLActiveStatus(string retailerId, bool activeState)
        {
            string result = "";
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", retailerId);
                    parameters.Add("@UPPCL_Active", activeState);
                    parameters.Add("@UPPCL_ActiveTime", DateTime.Now);
                    result = con.QuerySingleOrDefault<string>("usp_RetailUserUPPCLActUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    //result.RequestDomain = "success";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static string RTranUPPCLEWalletIdUpdate(string rtranId, string ewalletId)
        {
            RetailUser result = new RetailUser();
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", rtranId);
                    parameters.Add("@UPPCL_TransactionStatus", "SUCCESS");
                    parameters.Add("@UPPCL_TransactionId", ewalletId);
                    parameters.Add("@UPPCL_FundStatus", 3);
                    parameters.Add("@UPPCL_Status", "SUCCESS");
                    result = con.QuerySingleOrDefault<RetailUser>("usp_RTranUPPCLEWalletIdUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    result.RequestDomain = "success";
                }
            }
            catch (Exception ex)
            {
                result.RequestDomain = ex.Message;
            }
            return result.RequestDomain;
        }

        public static string GetApiResponseByApiTypeAndConsumerId(string consumerNumber, string apiType, string transId = null)
        {
            var result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ConsumerNumber", consumerNumber);
                    parameters.Add("@ApiType", apiType);
                    parameters.Add("@Transid", transId);
                    result = con.QuerySingleOrDefault<string>("usp_GetApiResponseByApiTypeAndConsumerId", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }


        public static string GetAllDistributorByWhiteLabel(string whiteLabelId)
        {
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@Id", whiteLabelId);
                    List<RetailUserGrid> allRetailUser = con.Query<RetailUserGrid>("usp_RetailUserListByWhiteLabel", queryParameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    //OperationMessage = saveResponse;
                    var aaData1 = new { data = allRetailUser };
                    return JsonConvert.SerializeObject(aaData1);
                }
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }

        public static string ROfferServerUpdate(int recordId, int recordStatus)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", recordId);
                    parameters.Add("@Active", recordStatus);
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_ROfferServerUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                    //Reloading for use in bill fetch code list update
                    StaticDatabaseData.LoadROfferServerMaster();
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Ex " + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string RTranUpdateData(string recordId)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", recordId);
                    List<PendingRechargeData> allClient = con.Query<PendingRechargeData>("usp_RTranPendingDetail", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { aadata = allClient };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: Ex " + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static PaymentReceipt PaymentReceiptByTranId(string tranId)
        {
            PaymentReceipt pr = new PaymentReceipt();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", tranId);
                using (var con = new SqlConnection(conString))
                {
                    pr = con.QuerySingleOrDefault<PaymentReceipt>("usp_PaymentReceiptById", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }

                if (pr.RechargeStatus == "FAILURE")
                {
                    pr.Amount = 0;
                    pr.LiveId = "PAYMENT FAILED";
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            return pr;
        }

        public static int RecordCount(string operatorName, string rechargeMobileNumber)
        {
            int recordCount = 0;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@OperatorName", operatorName);
                parameters.Add("@RechargeMobileNumber", rechargeMobileNumber);
                using (var con = new SqlConnection(conString))
                {
                    recordCount = con.QuerySingleOrDefault<OperationResponse>("usp_RecordCountForTheDay", parameters,commandType: System.Data.CommandType.StoredProcedure).RecordCount;
                }

            }
            catch (Exception)
            {

            }

            return recordCount;
        }

        public static PaymentReceiptUPPCL PaymentReceiptUPPCLByTranId(string tranId)
        {
            PaymentReceiptUPPCL pr = new PaymentReceiptUPPCL();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", tranId);
                using (var con = new SqlConnection(conString))
                {
                    pr = con.QuerySingleOrDefault<PaymentReceiptUPPCL>("usp_PaymentReceiptById", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }

                if (pr.RechargeStatus == "FAILURE")
                {
                    pr.Amount = 0;
                    pr.LiveId = "PAYMENT FAILED";
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            return pr;
        }

        public static OTSReciptModal PaymentOTSReceiptDataByTranId(string tranId)
        {
            OTSReciptModal pr = new OTSReciptModal();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", tranId);
                using (var con = new SqlConnection(conString))
                {
                    pr = con.QuerySingleOrDefault<OTSReciptModal>("usp_PaymentOTSReceiptById", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }

                if (pr.RechargeStatus == "FAILURE")
                {
                    pr.Amount = "0";
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            return pr;
        }

        public static string ReceiptByTranId(string tranId)
        {
            PaymentReceipt pr = new PaymentReceipt();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", tranId);
                using (var con = new SqlConnection(conString))
                {
                    pr = con.QuerySingleOrDefault<PaymentReceipt>("usp_PaymentReceiptById", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }

                if (pr.RechargeStatus == "FAILURE")
                {
                    pr.Amount = 0;
                    pr.LiveId = "PAYMENT FAILED";
                }
            }
            catch (Exception)
            {
                pr.LiveId = "INVALID DETAILS";
            }
            finally
            {

            }
            //var aaData = new { aadata = pr };
            var result = JsonConvert.SerializeObject(pr);
            return result;
        }

        public static string ResetPin(string op, string np, string retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@Password", op);
                parameters.Add("@NewPin", np);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserUpdatePin", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating pin.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string UpdateDefaultOperator(string op, string retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@DefaultUtilityOperator", op);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserDefaultOperator", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating password.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }
        
        public static string UpdateDefaultPrinter(string defaultPrinter, string retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@DefaultPrinter", defaultPrinter);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserDefaultPrinter", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating default printer.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string UpdatePassword(string op, string np, string retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@Password", op);
                parameters.Add("@NewPassword", np);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserUpdatePassword", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating password.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string SmsPassword(int retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserOrderNo", retailerId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserResendPassword", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            return result;
        }
        
        public static string SmsPass(string retailerMobile)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserMobile", retailerMobile);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserResendPasswordByMobile", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in sms password.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }
        
        public static string ResetPassword(int retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserOrderNo", retailerId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserResetPassword", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            return result;
        }

        public static string RetailUserPin(string retailUserId)
        {
            string retailuserPin = "";
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserId", retailUserId);
                    retailuserPin = con.QuerySingleOrDefault<string>("usp_GetUserPin", queryParameters, commandType: System.Data.CommandType.StoredProcedure);
                    retailuserPin = DecodePIN(retailuserPin);
                }
            }
            catch (Exception)
            {

            }
            return retailuserPin;
        }
        

        public static string UpdateRetailUserPin(string retailUserId, string pin)
        {
            string retailuserPin = "";
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserId", retailUserId);
                    queryParameters.Add("@Pin", EncodePIN(pin));
                    retailuserPin = con.QuerySingleOrDefault<OperationResponse>("usp_UpdateUserPin", queryParameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {

            }
            return retailuserPin;
        }

        public static string ResendRetailUserPin(string retailUserId, string pin)
        {
            string retailuserPin = "";
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var queryParameters = new DynamicParameters();
                    queryParameters.Add("@RetailUserId", retailUserId);
                    queryParameters.Add("@Pin", pin);
                    retailuserPin = con.QuerySingleOrDefault<OperationResponse>("usp_RetailUserResendPin", queryParameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {

            }
            return retailuserPin;
        }
        
        public static string TranListJson()
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    List<TransactionLog> transactionLogs = con.Query<TransactionLog>("usp_TransactionLog", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = JsonConvert.SerializeObject(transactionLogs);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        
        public static string RMonitorListJson(string a, int dt)
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@MonitorUserId",null);
                    parameters.Add("@MonitorUserMobile",a);
                    List<MonitorUserRetailUserWatch> transactionLogs = con.Query<MonitorUserRetailUserWatch>("usp_MonitorUserRetailUserWatch", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (dt == 1)
                    {
                        var aaData = new {data = transactionLogs};
                        result = JsonConvert.SerializeObject(aaData);
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(transactionLogs);
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        
        public static string WalletBalanceJson()
        {
            string result = "";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    WalletBalance walletBalance = con.Query<WalletBalance>("usp_WalletBalance", parameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    result = JsonConvert.SerializeObject(walletBalance);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        
        public static string UPPCLReportByDate(DateTime reportDateFrom, DateTime reportDateTo, int excelExport, string filePath = "")
        {
            var aaIData = new UPPCLReport();
            string result = JsonConvert.SerializeObject(aaIData);
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TranDateFrom", reportDateFrom.ToString("MM-dd-yyyy"));
                    parameters.Add("@TranDateTo", reportDateTo.ToString("MM-dd-yyyy"));
                    List<UPPCLReport> allDailyRecharge = con.Query<UPPCLReport>("usp_UPPCLReportByDate", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (excelExport==1)
                    {
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "UPPCLDailyTransaction", filePath);
                        
                    }
                    else
                    {
                        var aaData = new { data = allDailyRecharge };
                        result = JsonConvert.SerializeObject(aaData);
                    }
                }
            }
            catch (Exception ex)
            {
                
                result = JsonConvert.SerializeObject(aaIData);
            }
            finally
            {
                aaIData = null;
            }
            return result;
        }

        
        public static RecordSaveResponse RazorpayOrderSave(string name, decimal amount, string razorpayAmount, string email, string mobile, string retailerId, string provider = "Razor")
        {
            RecordSaveResponse result = new RecordSaveResponse();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Amount", amount);
                    parameters.Add("RazorpayAmount", razorpayAmount);
                    parameters.Add("@Currency", "INR");
                    parameters.Add("@CreateDate", DateTime.Now);
                    parameters.Add("@CustomerName", name);
                    parameters.Add("@CustomerMobile", mobile);
                    parameters.Add("@CustomerEmail", email);
                    parameters.Add("@RetailerId", retailerId);
                    parameters.Add("@Provider", provider);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        public static RecordSaveResponse RazorpayOrderUpdateOrderId(string id, string orderId)
        {
            RecordSaveResponse result = new RecordSaveResponse();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", id);
                    parameters.Add("@razorpay_order_id", orderId);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderUpdateOrderId", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        public static RecordSaveResponse RazorpayOrderUpdateFees(string orderId, string rfees, string rgst, string ofees, string pstatus = "")
        {
            RecordSaveResponse result = new RecordSaveResponse();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@razorpay_order_id", orderId);
                    parameters.Add("@RazorpayFees", rfees);
                    parameters.Add("@RazorpayGST", rgst);
                    parameters.Add("@OurFees", ofees);
                    parameters.Add("@OrderStatus", pstatus);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderUpdateFees", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        public static RecordSaveResponse RazorpayOrderUpdateOPS(string o, string p, string s)
        {
            RecordSaveResponse result = new RecordSaveResponse();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@razorpay_order_id", o);
                    parameters.Add("@razorpay_payment_id", p);
                    parameters.Add("@razorpay_signature", s);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_RazorPayOrderUpdateOPS", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        public static RazorpayOrder RazorpayOrderLoad(string id)
        {
            RazorpayOrder result = new RazorpayOrder();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", id);
                    result = con.QuerySingleOrDefault<RazorpayOrder>("usp_RazorPayOrderSelect", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        public static RazorpayOrder RazorpayOrderLoadByRazorpayId(string id)
        {
            RazorpayOrder result = new RazorpayOrder();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@razorpay_order_id", id);
                    result = con.QuerySingleOrDefault<RazorpayOrder>("usp_RazorPayOrderByRazorpayIdSelect", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }
        
        
        
        public static string RazorpayLogSave(string retailerId, string OrderId, string CustomerMobile, string RazorpayOrderId, string logType, string logData, DateTime CreateDate)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@OrderId", OrderId);
                    parameters.Add("@RetailerId", retailerId);
                    parameters.Add("@CustomerMobile", CustomerMobile);
                    parameters.Add("@RazorpayOrderId", RazorpayOrderId);
                    parameters.Add("@LogType", logType);
                    parameters.Add("@LogData", logData);
                    parameters.Add("@CreateDate", CreateDate);
                    result = con.QuerySingleOrDefault<string>("usp_RazorpayLogDataInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
        }

        public static DataTable JsonToDataTable(string jsonData)
        {
            try
            {
                return (DataTable)JsonConvert.DeserializeObject(jsonData, (typeof(DataTable)));
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public static string MapPath(string path)
        {
            return (string)AppDomain.CurrentDomain.GetData("WebRootPath");
            //return Path.Combine((string)AppDomain.CurrentDomain.GetData("ContentRootPath"), path);
        }
        
        public static string DataTableToExcelEP(DataTable dataTableExcel, string fileName, string filePath)
        {
            string result = "";
            try
            {
                string fileNameFull = fileName + "_" + DateTime.Now.ToString("ddMMMyy-HHmmss") + "_" + Guid.NewGuid().ToString() + ".xlsx";

                //filePath = string.IsNullOrEmpty(filePath) ? Path.Combine(_webHostEnvironment.WebRootPath,"FileData/" + fileName) : filePath;
                FileInfo fi = new FileInfo(filePath+fileNameFull);
                using (var excelPack = new ExcelPackage(fi)) 
                {
                    var ws = excelPack.Workbook.Worksheets.Add(fileName.Split('-')[0]);
                    ws.Cells.LoadFromDataTable(dataTableExcel, true, OfficeOpenXml.Table.TableStyles.Light2);
                    int colNumber = 1;
                    
                    
                    foreach (DataColumn col in dataTableExcel.Columns) 
                    {        
                        if (col.DataType == typeof(DateTime))
                        { 
                            ws.Column(colNumber).Style.Numberformat.Format = "dd-MM-yyyy HH:mm:ss";
                        }
                        //ws.Column(colNumber).AutoFit();
                        colNumber++;      
                    }
                    
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    excelPack.Save();
                }

                result = "<a href='/FileData/" + fileNameFull + "'" + " title='Download Excel File'>Download excel file.</a>";
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            finally
            {
                
            }
            return result;
        }
        
        
        public static string FirstString(string inputString)
        {
            if (inputString.IndexOf(" ") > 0)
            {
                inputString = inputString.Substring(0, inputString.IndexOf(" "));
            }
            return inputString.ToUpper();
        }
        
        public static async Task<string> WriteAppLog(string logToWrite, string logFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(logFilePath))
                {
                    //File.Create(LogFilePath).Close();
                    TextWriter tw = new StreamWriter(logFilePath);
                    await tw.WriteLineAsync(logToWrite);
                    tw.Close();
                }
                else if (System.IO.File.Exists(logFilePath))
                {
                    TextWriter tw = new StreamWriter(logFilePath, true);
                    await tw.WriteLineAsync(logToWrite);
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                return "Exception in saving log. " + ex.Message;
            }
            return "Save log done.";
        }

        public static string DistributorListJson(int userType)
        {
            string result = "[]";
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UserType", userType);
                    List<Distributor> allStates = con.Query<Distributor>("usp_GetDistributorList", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    result = JsonConvert.SerializeObject(allStates);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public static string UpdateActiveState(string retailerId, int updateChild)       
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@updateChild", updateChild);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_updateUserActivationState", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }

                bool hasActivatedIgnoreCase = Regex.IsMatch(result, @"\bactivated\b", RegexOptions.IgnoreCase);
                bool hasDeactivatedIgnoreCase = Regex.IsMatch(result, @"\bdeactivated\b", RegexOptions.IgnoreCase);

                if (hasActivatedIgnoreCase)
                {
                    var retailUser = UPPCLManager.RetailUserDetail(retailerId);
                    var activateAgent = UPPCLManager.AgentActivate(retailUser, AgentStatus.ACTIVE);
                    result += " Success: Done, current uppcl agent status - " + activateAgent.status;
                }
                else if (hasDeactivatedIgnoreCase)
                {
                    var retailUser = UPPCLManager.RetailUserDetail(retailerId);
                    var activateAgent = UPPCLManager.AgentActivate(retailUser, AgentStatus.INACTIVATE);
                    result += " Success: Done, current uppcl agent status - " + activateAgent.status;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating active state.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string UpdateKYCState(string retailerId, int docVerification, int activation, int docVerificationFailed, string failureReason)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@Active", activation);
                parameters.Add("@DocVerification", docVerification);
                parameters.Add("@DocVerificationFailed", docVerificationFailed);
                parameters.Add("@FailureReason", failureReason);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_updateKYCState", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating active state.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string UpdateDistributor(string retailerId, string distributorId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailUserId", retailerId);
                parameters.Add("@DistributorId", distributorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_updateDistributor", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception)
            {
                result = "Exception in updating active state.";// + ex.Message;
            }
            finally
            {

            }

            return result;
        }

        public static string SaveMonitorUser(string loginName, string loginPassword, string mobileNumber, string startTime, string endTime, string oldId, int active)
        {
            RecordSaveResponse result = new RecordSaveResponse();
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@LoginName", loginName);
                    parameters.Add("@LoginPassword", loginPassword);
                    parameters.Add("@MobileNumber", mobileNumber);
                    parameters.Add("@StartTime", startTime);
                    parameters.Add("@EndTime", endTime);
                    parameters.Add("@OldId", oldId);
                    parameters.Add("@Active", active);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_MonitorUserInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result.OperationMessage = "Errors: ExCodeNet " + ex.Message;
            }

            return result.OperationMessage; 
        }
        
       public static string ListMonitor()
       {
           string result;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    List<MonitorUser> muList = con.Query<MonitorUser>("usp_MonitorUserSelectAll", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = muList };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }
            return result;
       }
       
       public static string ListMapping()
       {
           string result;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    List<MonitorUserRetailUser> muList = con.Query<MonitorUserRetailUser>("usp_MonitorUserRetailUserALL", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    var aaData = new { data = muList };
                    result = JsonConvert.SerializeObject(aaData);
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
       }
       
       public static string UpdateMonitor(string id, string loginPassword, string mobileNumber, int active)
       {
           string result;
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", id);
                    parameters.Add("@LoginPassword", loginPassword);
                    parameters.Add("@MobileNumber", mobileNumber);
                    parameters.Add("@Active", active);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_MonitorUserUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
       }
       
       public static string UpdateMapping(string id, int? usl, string startTime, string endTime)
       {
           string result;
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@MonitorUserId", id);
                    parameters.Add("@RetailUserOrderNo", usl);
                    parameters.Add("@StartTime", startTime);
                    parameters.Add("@EndTime", endTime);
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_MonitorMappingInsert", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
       }
       
       public static string DeleteMapping(string id)
       {
           string result;
            try
            {
                
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", Convert.ToInt32(id));
                    result = con.QuerySingleOrDefault<RecordSaveResponse>("usp_MonitorUserRetailUserDelete", parameters, commandType: System.Data.CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
                result = "Errors: ExCodeNet " + ex.Message;
            }

            return result;
       }
       
       public static string GetRetailUserMonthlySummaries(DateTime startDate, DateTime endDate, int excelExport, string filePath = "")
       {
           var days = Enumerable.Range(0, (endDate - startDate).Days + 1)
               .Select(d => startDate.AddDays(d))
               .ToList();

           string dayColumns = string.Join(", ", days.Select(d => $"[{d:yyyy-MM-dd}]"));
           string dayValues = string.Join(", ", days.Select(d => $"[{d:yyyy-MM-dd}]"));

           using (var con = new SqlConnection(StaticData.conString))
           {
               var parameters = new DynamicParameters();
               parameters.Add("@StartDate", startDate);
               parameters.Add("@EndDate", endDate);
               var rows = con.Query("usp_DailySalesWithMaster", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
               DataTable dataTable = DapperConvertToDataTable(rows);

               if (excelExport==1)
               {
                   return DataTableToExcelEP(dataTable, "DailySalesByRetailUser", filePath);
               }
               else
               {
                   var aaData = new { data = rows };
                   return JsonConvert.SerializeObject(aaData);
               }
                
           }
       }
       
       public static string GetDailyBusinessReport(DateTime startDate)
       { 
           using (var con = new SqlConnection(conString))
           {
               var parameters = new DynamicParameters();
               parameters.Add("@StartDate", startDate);
               var rows = con.Query("usp_DailyBusinessReport", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
               
               var aaData = new { data = rows };
               return JsonConvert.SerializeObject(aaData);
           }
       }
       
       public static string GetGrowthReportSummary(DateTime startDate, DateTime endDate, int x, string filePath)
       { 
           using (var con = new SqlConnection(conString))
           {
               var parameters = new DynamicParameters();
               parameters.Add("@StartDate", startDate);
               parameters.Add("@EndDate", endDate);
               List<GrowthSummary> rows = con.Query<GrowthSummary>("usp_GetRechargeSummaryByRetailUser", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
               
               if (x==1)
               {
                   return DataTableToExcelEP(rows.ToDataTable(), "GrowthReportSummary", filePath);
               }
               else
               {
                   var aaData = new { data = rows };
                   return JsonConvert.SerializeObject(aaData);
               }
               
           }
       }
       
       public static DataTable DapperConvertToDataTable(IEnumerable<dynamic> dapperRows)
       {
           DataTable dataTable = new DataTable();
           
           if (dapperRows == null)
           {
               throw new ArgumentNullException(nameof(dapperRows), "DapperRow list cannot be null");
           }

           bool columnsAdded = false;

           foreach (var row in dapperRows)
           {
               if (!columnsAdded)
               {
                   foreach (var column in (IDictionary<string, object>)row)
                   {
                       dataTable.Columns.Add(column.Key, column.Value?.GetType() ?? typeof(object));
                   }
                   columnsAdded = true;
               }
               
               var newRow = dataTable.NewRow();
               foreach (var column in (IDictionary<string, object>)row)
               {
                   newRow[column.Key] = column.Value ?? DBNull.Value;
               }
               dataTable.Rows.Add(newRow);
           }

           return dataTable;
       }

        public static List<UserInfo> GetUsersByUserType(int userType)
        {
            List<UserInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserType", userType);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<UserInfo>("usp_GetUsersByUserType", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static List<UserInfo> GetMappedUsers()
        {
            List<UserInfo> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<UserInfo>("usp_GetMappedUsers", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static List<MappedUserInfo> GetMappedUsersByCollectorId(string userId)
        {
            List<MappedUserInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<MappedUserInfo>("usp_GetMappedUsersByCollectorId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static UserInfo CashFlowLogin(string userId, string password)
        {
            UserInfo result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                parameters.Add("@password", password);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<UserInfo>("usp_CashFlowLogin", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static MasterData GetMasterData()
        {
            MasterData result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var statuses = con.Query<Status>("usp_GetMasterData", commandType: CommandType.StoredProcedure).ToList();
                    result.WorkFlows = new();
                    result.TransactionTypes = new();
                    result.WorkFlows.AddRange(statuses.Where(_ => _.Type == "WorkFlow"));
                    result.TransactionTypes.AddRange(statuses.Where(_ => _.Type == "TransactionType"));
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static string AlignCollectorWithRetailerUser(string collectorId, string retailerId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", collectorId);
                parameters.Add("@RetailerId", retailerId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("Usp_AlignCollectorWithRetailerUser", parameters, commandType: CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static string DeleteLadgerInfo(int id)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_DeleteLadgerInfo", parameters, commandType: CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static LiabilityInfo GetLiabilityAmountByRetailerId(string userId)
        {
            LiabilityInfo result = new LiabilityInfo();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<LiabilityInfo>("Usp_GetLiabilityAmountByRetailerId", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static LiabilityInfo GetLiabilityAmountByCollectorId(string userId)
        {
            LiabilityInfo result = new LiabilityInfo();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<LiabilityInfo>("Usp_GetLiabilityAmountByCollectorId", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static LiabilityInfo GetLiabilityAmountByCashierId(string userId)
        {
            LiabilityInfo result = new LiabilityInfo();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<LiabilityInfo>("Usp_GetLiabilityAmountByCashierId", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static List<MappedUserInfo> GetMappedCollectorsByRetailerId(string userId)
        {
            List<MappedUserInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<MappedUserInfo>("usp_GetMappedCollectorsByRetailerId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static List<LiabilityInfo> GetLiabilityAmountOfAllRetailers(DateTime date)
        {
            List<LiabilityInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Date", date);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<LiabilityInfo>("Usp_GetLiabilityAmountOfAllRetailers", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static bool AddLadgerInfo(LadgerInfo ladger)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@RetailerId", ladger.RetailerId);
                parameters.Add("@CollectorId", ladger.CollectorId);
                parameters.Add("@Amount", ladger.Amount);
                parameters.Add("@TransactionType", ladger.TransactionType);
                parameters.Add("@WorkFlow", ladger.WorkFlow);
                parameters.Add("@Date", ladger.Date);
                parameters.Add("@GivenOn", ladger.GivenOn);
                parameters.Add("@Comment", ladger.Comment);
                parameters.Add("@CashierId", ladger.CashierId);
                parameters.Add("@DocId", ladger.DocId);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("Usp_AddLadgerInfo", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool UpdateLadgerInfo(LadgerInfo ladger)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", ladger.Id);
                parameters.Add("@RetailerId", ladger.RetailerId);
                parameters.Add("@CollectorId", ladger.CollectorId);
                parameters.Add("@Amount", ladger.Amount);
                parameters.Add("@TransactionType", ladger.TransactionType);
                parameters.Add("@WorkFlow", ladger.WorkFlow);
                parameters.Add("@Date", ladger.Date);
                parameters.Add("@GivenOn", ladger.GivenOn);
                parameters.Add("@Comment", ladger.Comment);
                parameters.Add("@DocId", ladger.DocId);
                parameters.Add("@CashierId", ladger.CashierId);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("Usp_UpdateLadgerInfo", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        internal static List<Ladger> GetLadgerInfoByRetailerid(bool all, string retailerId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@All", all);
                parameters.Add("@RetailerId", retailerId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("Usp_GetLadgerInfoByRetailerid", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetLadgerInfoByCollectorId(bool all, string collectorId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@All", all);
                parameters.Add("@CollectorId", collectorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("Usp_GetLadgerInfoByCollectorId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetLadgerInfoCreatedByCashierId(bool all, string cashierId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@All", all);
                parameters.Add("@CashierId", cashierId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("Usp_GetLadgerInfoCreatedByCashierId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetLadgerInfoByRetaileridAndCollectorId(bool all, string retailerId, string collectorId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@All", all);
                parameters.Add("@RetailerId", retailerId);
                parameters.Add("@CollectorId", collectorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("Usp_GetLadgerInfoByRetaileridAndCollectorId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static string GetGrowthReportData(DateTime reportDate, string filePath, string[] removeColumnsFromExcel)
        {
            string result = "";
            DataSet ds = new DataSet("GrowthReports");
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetGrowthReportData", con);
                    sqlComm.Parameters.AddWithValue("@ReportDate", reportDate);

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;

                    da.Fill(ds);
                    result = DataSetToExcelEP(ds, "GrowthReport", filePath, removeColumnsFromExcel);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static string DataSetToExcelEP(DataSet dataSet, string fileName, string filePath, string[] removeColumnsFromExcel)
        {
            string result = "";
            try
            {
                string fileNameFull = fileName + "_" + DateTime.Now.ToString("ddMMMyy-HHmmss") + "_" + Guid.NewGuid().ToString() + ".xlsx";

                //filePath = string.IsNullOrEmpty(filePath) ? Path.Combine(_webHostEnvironment.WebRootPath,"FileData/" + fileName) : filePath;
                FileInfo fi = new FileInfo(filePath + fileNameFull);

                if(removeColumnsFromExcel != null && removeColumnsFromExcel.Any())
                {
                    foreach(var column in removeColumnsFromExcel)
                    {
                        dataSet.Tables[1].Columns.Remove(column);
                    }
                }
                using (var excelPack = new ExcelPackage(fi))
                {
                    var ws = excelPack.Workbook.Worksheets.Add(fileName.Split('-')[0]);
                    ws.Cells.LoadFromDataTable(dataSet.Tables[1], true, OfficeOpenXml.Table.TableStyles.Light2);
                    int colNumber = 1;


                    foreach (DataColumn col in dataSet.Tables[1].Columns)
                    {
                        if (col.DataType == typeof(DateTime))
                        {
                            ws.Column(colNumber).Style.Numberformat.Format = "dd-MM-yyyy HH:mm:ss";
                        }
                        //ws.Column(colNumber).AutoFit();
                        colNumber++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    
                    ws.InsertRow(1, 1);
                    ws.Cells["A1:AJ1"].Value = dataSet.Tables[0].Rows[0][0].ToString();
                    ws.Cells["A1:AJ1"].Merge = true;
                    ws.Cells["A1:AJ1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["A1:AJ1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Row(1).Style.Font.Bold = true;
                    ws.Row(1).Style.Font.Color.SetColor( System.Drawing.Color.Red);
                    ws.Row(1).Height = 30;
                    excelPack.Save();
                }

                result = "<a href='/FileData/" + fileNameFull + "'" + " title='Download Excel File'>Download excel file.</a>";
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            finally
            {

            }
            return result;
        }

        internal static List<Ladger> GetLadgerInfosCreatedByCollectors(DateTime date)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Date", date);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetLadgerInfosCreatedByCollectors", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<LiabilityInfo> GetCollectorLiabilities()
        {
            List<LiabilityInfo> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<LiabilityInfo>("usp_GetCollectorLiabilities", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetCollectorLiabilityDetails(string collectorId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", collectorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetCollectorLiabilityDetails", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetCashierLiabilityDetails(string cashierId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CashierId", cashierId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetCashierLiabilityDetails", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetCollectorLedgerDetails(string collectorId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", collectorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetCollectorLedgerDetails", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetCashierLedgerDetails(string cashierId)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CashierId", cashierId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetCashierLedgerDetails", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetPendingApprovalLedgers(bool showAll, int userType)
        {
            List<Ladger> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ShowAll", showAll);
                    parameters.Add("@UserType", userType);
                    result = con.Query<Ladger>("usp_GetPendingApprovalLedgers", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<UserEx> GetUserExtendedInfo()
        {
            List<UserEx> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<UserEx>("usp_GetUserExtendedData", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<CollectorInfo> GetLinkedCollectors(string userId)
        {
            List<CollectorInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<CollectorInfo>("usp_GetLinkedCollectors", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static bool UpdateIsSelfSubmitterFlag(SubmitterFlagData data)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", data.UserId);
                parameters.Add("@IsSelfSubmitter", data.IsSelfSubmitter);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("usp_InsertOrUpdateIsSelfSubmitter", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        internal static bool UpdateIsThirdPartyFlag(ThirdpartyFlagData data)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", data.UserId);
                parameters.Add("@IsThirdParty", data.IsThirdParty);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("usp_InsertOrUpdateIsThirdParty", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        internal static bool UpdateOpeningBalanceData(OpeningBalanceData data)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", data.UserId);
                parameters.Add("@OpeningBalance", data.OpeningBalance);
                parameters.Add("@OpeningBalanceDate", data.OpeningBalanceDate);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("usp_InsertOrUpdateOpeningBalance", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        internal static bool LinkAllRetailesToNewCollector(LinkingInfo data)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FromCollectorId", data.FromCollectorId);
                parameters.Add("@ToCollectorId", data.ToCollectorId);
                using (var con = new SqlConnection(conString))
                {
                    con.Execute("usp_LinkAllRetailesToNewCollector", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static List<LiabilityInfo> GetLiabilityAmountOfAllRetailers()
        {
            List<LiabilityInfo> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<LiabilityInfo>("Usp_GetLiabilityAmountOfAllRetailers", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<LiabilityInfo> GetLiabilityAmountOfAllRetailersByCollectorId(string collectorId)
        {
            List<LiabilityInfo> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", collectorId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<LiabilityInfo>("Usp_GetLiabilityAmountOfAllRetailersByCollectorId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<Ladger> GetPendingApprovalLedgersByCollectorId(string collectorId, bool showAll)
        {
            List<Ladger> result = new();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", collectorId);
                parameters.Add("@ShowAll", showAll);
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<Ladger>("usp_GetPendingApprovalLedgersByCollectorId", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static string GetPassword(string userId)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("usp_GetPassword", parameters, commandType: CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static string DeleteLinking(CollectorRetailerMapping data)
        {
            string result = string.Empty;
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CollectorId", data.CollectorId);
                parameters.Add("@RetailerId", data.RetailerId);
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<OperationResponse>("Usp_DeleteLinking", parameters, commandType: CommandType.StoredProcedure).OperationMessage;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static List<LiabilityInfo> GetCashierLiabilities()
        {
            List<LiabilityInfo> result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.Query<LiabilityInfo>("usp_GetCashierLiabilities", commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static Highlights GetHighlights()
        {
            Highlights result = new();
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    result = con.QuerySingleOrDefault<Highlights>("usp_GetHighlights", commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        internal static bool UpdateHighlights(string message)
        {
            bool result = true;
            try
            {
                using (var con = new SqlConnection(conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Message", message);
                    con.Execute("usp_UpdateHighlights", parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
