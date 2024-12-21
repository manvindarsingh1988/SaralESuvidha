using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using SaralESuvidha.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using SpreadsheetLight;
using UPPCLLibrary;
using UPPCLLibrary.BillFetch;
using RTran = SaralESuvidha.Models.RTran;
using DocumentFormat.OpenXml.Bibliography;
using System.Drawing;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Asn1.Ocsp;
using UPPCLLibrary.OTS;

namespace SaralESuvidha.ViewModel
{
    public static class StaticData
    {
        public static string conString = Startup.ConnectionString;
        public static string logFileName = DateTime.Now.ToString("yy-MM-dd") + "_ajax.txt";
        public static string loginSource = "web";
        public static string rofferMobileUrl = "";
        public static string rofferDthCustInfoUrl = "";
        public static string rzp_ApiKey = "rzp_live_gMiAscmFhv8wrv";
        public static string rzp_ApiSecret = "jtX8wTfpArPLse0PerpTkthO";

        const int ColumnBase = 26;
        const int DigitMax = 7; // ceil(log26(Int32.Max))
        const string Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static RetailUser retailUser = new RetailUser();
        public static SystemSetting systemSetting = new SystemSetting();
        
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

        public static string ElectricityBillInfoUPPCL(string operatorName, string accountNumber)
        {
            UPPCLManager.Initialize();
            string result = string.Empty;
            operatorName = UPPCLOperatorName(operatorName);
            try
            {
                ESuvidhaBillFetchResponse eSuvidhaBillFetchResponse = new ESuvidhaBillFetchResponse();
                
                //System Down Message in case of maintenance or uppcl issue
                //LoadSystemSetting();
                if (systemSetting.IsDown == true)
                {
                    eSuvidhaBillFetchResponse.Reason = "Errors: " + systemSetting.IsDownMessage;
                    eSuvidhaBillFetchResponse.CanNotPay = true;
                    eSuvidhaBillFetchResponse.PayAmount = 0;
                    eSuvidhaBillFetchResponse.MaximumPayAmount = 0;
                    eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount = "0";
                }
                else
                {
                    eSuvidhaBillFetchResponse.BillFetchResponse = UPPCLManager.BillFetch(operatorName, accountNumber);
                    eSuvidhaBillFetchResponse.ValidateBill();
                    
                    if (eSuvidhaBillFetchResponse.Reason == "Invalid Credentials")
                    {
                        UPPCLManager.BillFetchToken();
                        eSuvidhaBillFetchResponse.BillFetchResponse = UPPCLManager.BillFetch(operatorName, accountNumber);
                        eSuvidhaBillFetchResponse.ValidateBill();
                    }

                    if (eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.ProjectArea == "NA")
                    {
                        eSuvidhaBillFetchResponse.Reason = "Errors: Invalid project area NA, can not accept bill, please try after some time.";
                        eSuvidhaBillFetchResponse.CanNotPay = true;
                        eSuvidhaBillFetchResponse.PayAmount = 0;
                        eSuvidhaBillFetchResponse.MaximumPayAmount = 0;
                        eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount = "0";
                    }
                
                    if (string.IsNullOrEmpty(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillID))
                    {
                        eSuvidhaBillFetchResponse.Reason = "Errors: Invalid bill id, missing, can not accept bill, please try after some time.";
                        eSuvidhaBillFetchResponse.CanNotPay = true;
                        eSuvidhaBillFetchResponse.PayAmount = 0;
                        eSuvidhaBillFetchResponse.MaximumPayAmount = 0;
                        eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount = "0";
                    }
                    /*
                    if (string.IsNullOrEmpty(eSuvidhaBillFetchResponse.BillFetchResponse.FAULTCODE))
                    {
                        eSuvidhaBillFetchResponse.Reason = "Errors: Fault: " + eSuvidhaBillFetchResponse.BillFetchResponse.FAULTSTRING;
                        eSuvidhaBillFetchResponse.CanNotPay = true;
                        eSuvidhaBillFetchResponse.PayAmount = 0;
                        eSuvidhaBillFetchResponse.MaximumPayAmount = 0;
                        eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount = "0";
                    }
                    */
                    
                    if (eSuvidhaBillFetchResponse.BillFetchResponse.FAULTCODE == "211")
                    {
                        eSuvidhaBillFetchResponse.Reason = "Errors: " + eSuvidhaBillFetchResponse.BillFetchResponse.FAULTSTRING;
                        eSuvidhaBillFetchResponse.CanNotPay = true;
                        eSuvidhaBillFetchResponse.PayAmount = 0;
                        eSuvidhaBillFetchResponse.MaximumPayAmount = 0;
                        eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount = "0";
                    }
                }

                result = JsonConvert.SerializeObject(eSuvidhaBillFetchResponse);

            }
            catch (Exception ex)
            {
                result = "BI - " + ex.Message;
            }
            finally
            {

            }

            return result;
        }
        
        public static string ElectricityBillInfo(string operatorName, string accountNumber)
        {
            string result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@OperatorName", operatorName);
                    ROfferServer ros = con.Query<ROfferServer>("usp_GetROfferServer", parameters, commandType: System.Data.CommandType.StoredProcedure).SingleOrDefault();
                    if (ros != null)
                    {
                        if (ros.ROfferApiOperatorName == "NA")
                        {
                            ElectricityBillInfo eb = new ElectricityBillInfo { AccountNumber = accountNumber, DetailedOperatorName = ros.DetailedOperatorName, OperatorBiller = ros.OperatorName, Message = "Can not fetch bill. Server issue." };
                            result = JsonConvert.SerializeObject(eb);
                            eb = null;
                        }
                        else
                        {
                            string url = "";
                            if (StaticDatabaseData.rOfferServerMasterList.Count == 0)
                            {
                                StaticDatabaseData.LoadROfferServerMaster();
                            }

                            url = StaticDatabaseData.rOfferServerMasterList.SingleOrDefault(r =>
                                r.ROfferServerName == ros.ROfferApiName && r.OperatorType == ros.OperatorType &&
                                r.Active == true)
                                ?.ServerUrl;
                            //if (string.IsNullOrEmpty(StaticData.systemSetting.ElectricityBillInfoUrl))
                            //{
                            //    StaticData.LoadSystemSetting();
                            //}
                            //url = StaticData.systemSetting.ElectricityBillInfoUrl;

                            url = url.Replace("<operator>", ros.ROfferApiOperatorName)
                                .Replace("<number>", accountNumber)
                                .Replace("<type>", "electricity");
                            //url = "http://203.112.148.237:82/act/ro.aspx?o=" + ros.ROfferApiOperatorName + "&n=" + accountNumber + "&t=electricity";
                            result = ReadURL(url);
                            ElectricityBillInfo ebi = JsonConvert.DeserializeObject<ElectricityBillInfo>(result);
                            ebi.DetailedOperatorName = ros.DetailedOperatorName;
                            if (!string.IsNullOrEmpty(ebi.EarlyPaymentBillAmount))
                            {
                                ebi.BillAmount = ebi.EarlyPaymentBillAmount;
                            }
                            if (!string.IsNullOrEmpty(ebi.PayableAmount))
                            {
                                ebi.BillAmount = ebi.PayableAmount;
                            }
                            result = JsonConvert.SerializeObject(ebi);
                            ebi = null;
                            ros = null;
                        }
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(new ElectricityBillInfo { Message = "Invalid operator." });
                    }
                }
            }
            catch (Exception ex)
            {
                result = "BI - " + ex.Message;
            }
            finally
            {

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
                if (retailClientOrderNo > 0)
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

        public static string AllPnLReportResultByUserAndDate(int excelExport, DateTime dateFrom, DateTime dateTo, int id, string filePath = "")
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
                    List<AllUserWithBalance> allDailyRecharge = con.Query<AllUserWithBalance>("usp_RetailUserListWithUPPCLCommission", parameters, commandType: System.Data.CommandType.StoredProcedure).ToList();
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
                        result = DataTableToExcelEP(allDailyRecharge.ToDataTable(), "DailyAllRecharge", filePath);
                        
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

        public static string GetApiResponseByApiTypeAndConsumerId(string consumerNumber, string apiType)
        {
            var result = string.Empty;
            try
            {
                using (var con = new SqlConnection(StaticData.conString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ConsumerNumber", consumerNumber);
                    parameters.Add("@ApiType", apiType);
                    result = con.QuerySingleOrDefault<string>("usp_GetApiResponseByApiTypeAndConsumerId", parameters, commandType: System.Data.CommandType.StoredProcedure);
                    
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public static (string, string) PayOTSUPPCL(string operatorName, string accountNumber, string retailerId, string retailUserOrderNo, string requestIp, CaseInitResponse initResponse, string userAgent, decimal amount, decimal outStandingAmount, string pi = "", string inputSource = "web")
        {
            var billTran = new RTran();
            string result = string.Empty;
            string reciptId = string.Empty;
            try
            {
                string pin = StaticData.RetailUserPin(retailerId);
                if (pi != "pintest")
                {
                    pi = StaticData.ConvertHexToString(pi);
                }
                else
                {
                    pi = pin;

                }

                if (pi != pin)
                {
                    result = "Errors: Invalid pin.";
                }
                else
                {
                    billTran.IsOTS = 1;
                    billTran.RetailUserOrderNo = Convert.ToInt32(retailUserOrderNo);
                    billTran.RetailUserId = retailerId;
                    billTran.TelecomOperatorName = operatorName;
                    billTran.RechargeMobileNumber = accountNumber;
                    billTran.Amount = amount;
                    var dueAmount = (int)Math.Round(Convert.ToDecimal(initResponse.Data.BillDetails.AccountInfo), MidpointRounding.AwayFromZero);
                    string retailerName = "" + dueAmount.ToString();
                    try
                    {
                        retailerName += " >> " + retailUserOrderNo;
                        retailerName += "-" + retailerName;
                        retailerName = retailerName.Length > 48 ? retailerName.Substring(0, 46) : retailerName;
                    }
                    catch (Exception exName)
                    {
                        return ("Errors: Invalid consumer name in bill fetch." + exName.Message, reciptId);
                    }
                    billTran.Parameter4 = retailerName;
                    billTran.RequestIp = requestIp;
                    billTran.RequestMachine = inputSource + ">" + userAgent;
                    billTran.EndCustomerName = initResponse.Data.BillDetails.ConsumerName;
                    billTran.EndCustomerMobileNumber = initResponse.Data.BillDetails.MobileNumber;
                    billTran.Extra1 = initResponse.Data.BillDetails.BillDueDate;
                    billTran.Extra2 = dueAmount.ToString();
                    billTran.UPPCL_ProjectArea = initResponse.Data.BillDetails.ProjectArea;
                    if (decimal.TryParse(initResponse.Data.BillDetails.Param1, out var res))
                    {
                        billTran.UPPCL_DDR = res;
                    }
                    try
                    {
                        billTran.UPPCL_AccountInfo = Convert.ToDecimal(initResponse.Data.BillDetails.AccountInfo);
                    }
                    catch (Exception exAccInfo)
                    {
                        return ("Errors: Invalid Account Info." + exAccInfo.Message, reciptId);
                    }
                    billTran.UPPCL_TDConsumer = initResponse.Data.BillDetails.TdStatus == "true" ? true : false;
                    billTran.UPPCL_ConnectionType = initResponse.Data.BillDetails.ConnectionType;
                    billTran.UPPCL_DivCode = initResponse.Data.BillDetails.DivCode;
                    billTran.UPPCL_SDOCode = initResponse.Data.BillDetails.SdoCode;
                    try
                    {
                        billTran.UPPCL_BillAmount = Convert.ToDecimal(initResponse.Data.BillDetails.BillAmount);
                    }
                    catch (Exception exBillAmount)
                    {
                        return ("Errors: Invalid Bill Amount." + exBillAmount.Message, reciptId);
                    }
                    billTran.UPPCL_Division = initResponse.Data.BillDetails.Division;
                    billTran.UPPCL_SubDivision = initResponse.Data.BillDetails.SubDivision;
                    billTran.UPPCL_PurposeOfSupply = initResponse.Data.BillDetails.PurposeOfSupply;
                    try
                    {
                        billTran.UPPCL_SanctionedLoadInKW = Convert.ToDecimal(initResponse.Data.BillDetails.SanctionedLoadInKW);
                    }
                    catch (Exception exSanLoad)
                    {
                        return ("Errors: Invalid Sanctioned Load." + exSanLoad.Message, reciptId);
                    }
                    billTran.UPPCL_BillId = initResponse.Data.BillDetails.BillId;
                    billTran.UPPCL_BillDate = initResponse.Data.BillDetails.BillDate;
                    billTran.UPPCL_Discom = operatorName;
                    var res1 = billTran.PayOTSUPPCL(initResponse, outStandingAmount, inputSource);
                    result = res1.Item1;
                    reciptId = res1.Item2;
                }
            }
            catch (Exception ex)
            {
                string extra = "opname-" + operatorName;
                result += "Errors: Exception: BPU: " + ex.Message;
            }
            finally
            {
                billTran = null;
            }

            if (result == null)
            {
                result = "Errors: Error in paying bill, null response, please check status after 5 minutes.";
            }
            else
            {
                if (result.Contains("Successfully updated to failed"))
                {
                    result = "Errors: Can not pay bill, please try later.";
                }
            }
            return (result, reciptId);
        }


        public static string PayBillUPPCL(string operatorName, string accountNumber, decimal billAmount, string retailerId, string eBillInfo, string retailUserOrderNo, string retailUserName, string userAgent, string requestIp, string additionalInfo1 = "", string customerName = null, string dueDate = null, string dueAmount = null, string p1 = "", string p2 = "", string inputSource = "web", string pi="", string clientReferenceId = "")
        {
            var billTran = new RTran();
            string result = string.Empty;
            string pin = StaticData.RetailUserPin(retailerId);
            if (pi != "pintest")
            {
                pi = StaticData.ConvertHexToString(pi);
            }
            else
            {
                pi = pin;
                
            }

            if(pi != pin)
            {
                result = "Errors: Invalid pin.";
            }
            else
            {
                operatorName = StaticData.UPPCLOperatorName(operatorName);

                try
                {
                    ESuvidhaBillFetchResponse eSuvidhaBillFetchResponse = JsonConvert.DeserializeObject<ESuvidhaBillFetchResponse>(eBillInfo);

                    if (eSuvidhaBillFetchResponse != null)
                    {
                        UPPCLManager.Initialize();
                        decimal walletBalance = Convert.ToDecimal(UPPCLManager.WalletDetails().balance);

                        if (1!=1) //billAmount > walletBalance
                        {
                            result = "Errors: Insufficient wallet balance. Can not accept request.";
                        }
                        else
                        {
                            if (eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.KNumber == accountNumber)
                            {
                                if (billAmount > 0)
                                {
                                    if (billAmount < eSuvidhaBillFetchResponse.MinimumPayAmount)
                                    {
                                        result = "Errors: Invalid amount. Minimum payable bill amount is - " + eSuvidhaBillFetchResponse.MinimumPayAmount + ".";
                                    }
                                    else if (billAmount > eSuvidhaBillFetchResponse.MaximumPayAmount || billAmount >= 200000)
                                    {
                                        result = "Errors: Invalid amount. Maximum payable bill amount is - " + eSuvidhaBillFetchResponse.MaximumPayAmount + ".";
                                    }
                                    else
                                    {
                                        string retailerName = "" + (dueAmount != null ? dueAmount.ToString() : "0");
                                        try
                                        {
                                            retailerName += " >> " + retailUserOrderNo;
                                            retailerName += "-" + retailerName;
                                            retailerName = retailerName.Length > 48 ? retailerName.Substring(0, 46) : retailerName;
                                        }
                                        catch (Exception exName)
                                        {
                                            return "Errors: Invalid consumer name in bill fetch." + exName.Message;
                                        }

                                        //string mobileNumber = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.MobileNumber;
                                        string mobileNumber = additionalInfo1;
                                        
                                        try
                                        {
                                            if (!string.IsNullOrEmpty(mobileNumber))
                                            {
                                                mobileNumber = mobileNumber.Length > 18 ? mobileNumber.Substring(0, 17) : mobileNumber;
                                            }
                                        }
                                        catch (Exception exMo)
                                        {
                                            return "Errors: Invalid mobile number in bill fetch." + exMo.Message;
                                        }

                                        //HttpContext.Session.SetInt32("RetailUserOrderNo", 12);
                                        billTran.RetailUserOrderNo = Convert.ToInt32(retailUserOrderNo);
                                        billTran.RetailUserId = retailerId;
                                        billTran.TelecomOperatorName = operatorName;
                                        billTran.RechargeMobileNumber = accountNumber;
                                        billTran.Amount = billAmount;
                                        billTran.Parameter1 = p1;
                                        billTran.Parameter2 = p2;
                                        billTran.Parameter3 = additionalInfo1;
                                        billTran.Parameter4 = retailerName;
                                        billTran.RequestIp = requestIp;
                                        billTran.RequestMachine = inputSource + ">" + userAgent;
                                        billTran.EndCustomerName = customerName;
                                        billTran.EndCustomerMobileNumber = mobileNumber;
                                        billTran.Extra1 = dueDate;
                                        billTran.Extra2 = dueAmount;
                                        billTran.UPPCL_ProjectArea = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.ProjectArea;
                                        try
                                        {
                                            billTran.UPPCL_AccountInfo = Convert.ToDecimal(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.AccountInfo);
                                        }
                                        catch (Exception exAccInfo)
                                        {
                                            return "Errors: Invalid Account Info." + exAccInfo.Message;
                                        }
                                        billTran.UPPCL_TDConsumer = eSuvidhaBillFetchResponse.TemporaryDisconnection;
                                        billTran.UPPCL_ConnectionType = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.ConnectionType;
                                        billTran.UPPCL_DivCode = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.DivCode;
                                        billTran.UPPCL_SDOCode = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.SDOCode;
                                        try
                                        {
                                            billTran.UPPCL_BillAmount = Convert.ToDecimal(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillAmount);
                                        }
                                        catch (Exception exBillAmount)
                                        {
                                            return "Errors: Invalid Bill Amount." + exBillAmount.Message;
                                        }
                                        billTran.UPPCL_Division = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.Division;
                                        billTran.UPPCL_SubDivision = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.SubDivision;
                                        billTran.UPPCL_PurposeOfSupply = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.PurposeOfSupply;
                                        try
                                        {
                                            billTran.UPPCL_SanctionedLoadInKW = Convert.ToDecimal(eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.SanctionedLoadInKW);
                                        }
                                        catch (Exception exSanLoad)
                                        {
                                            return "Errors: Invalid Sanctioned Load." + exSanLoad.Message;
                                        }
                                        billTran.UPPCL_BillId = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillID;
                                        billTran.UPPCL_BillDate = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.BillDate;
                                        billTran.UPPCL_Discom = operatorName;
                                        //billTran.UPPCL_Discom = eSuvidhaBillFetchResponse.BillFetchResponse.Body.PaymentDetailsResponse.Discom;

                                        result = billTran.PayBillUPPCL(eSuvidhaBillFetchResponse, inputSource, clientReferenceId);
                                        
                                    }

                                }
                                else
                                {
                                    return "Errors: Invalid bill amount.";
                                }
                            }
                            else
                            {
                                result = "Errors: Invalid bill details, data mismatch, please check bill again.";
                            }
                        }
                    }
                    else
                    {
                        result = "Errors: Invalid bill details, blank data, please check bill again.";
                    }

                }
                catch (Exception ex)
                {
                    string extra = "pi - " + pi + ", pin - " + pin + "opname-" + operatorName;
                    extra += " ai-" + additionalInfo1 + " p1-" + p1; 
                    result += "Errors: Exception: BPU: " + ex.Message;
                }
                finally
                {
                    billTran = null;
                }
            }

            if (result == null)
            {
                result = "Errors: Error in paying bill, null response, please check status after 5 minutes.";
            }
            else
            {
                if (result.Contains("Successfully updated to failed"))
                {
                    result = "Errors: Can not pay bill, please try later.";
                }
            }
            return result; //"Bill paid for " + operatorName + " - " + accountNumber
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

        
        public static RecordSaveResponse RazorpayOrderSave(string name, decimal amount, string razorpayAmount, string email, string mobile, string retailerId)
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
       
    }
}
