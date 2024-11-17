using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.ViewModel;

namespace SaralESuvidha.Models
{
    public class ApiConfigOperator
    {
        public string ID { get; set; }
        public string ApiId { get; set; }
        public string ApiName { get; set; }
        public string TelecomOperatorName { get; set; }
        public string InternalOperatorCode { get; set; }
        public string ApiOperatorCode { get; set; }
        /// <summary>
        /// 1=Percent,2=Fixed
        /// </summary>
        public byte? MarginType { get; set; }
        /// <summary>
        /// 1=Credit,2=Debit
        /// </summary>
        public byte? MarginCreditDebit { get; set; }
        public decimal? MarginAmount { get; set; }
        public string RechargeType { get; set; }
        public bool OtherApiRechargeTypeEnabled { get; set; }
        public string OtherApiRechargeType { get; set; }
        public string Gateway { get; set; }
        public short? RecordCount { get; set; }
        public string SortOrder { get; set; }
        public bool IsCustomPushEnabled { get; set; }
        public string CustomPushUrl { get; set; }
        public string CustomPushHeader { get; set; }
        public string CustomPushBodyData { get; set; }
        public bool AdditionalPara1Enabled { get; set; }
        public string AdditionalPara1 { get; set; }
        public bool AdditionalPara2Enabled { get; set; }
        public string AdditionalPara2 { get; set; }
        public bool Parameter1Enabled { get; set; }
        public bool Parameter2Enabled { get; set; }
        public bool Parameter3Enabled { get; set; }
        public bool Active { get; set; }
        public bool IsCustomTransactionIdEnabled { get; set; }
        public short? CustomTransactionID { get; set; }
        public string IdPattern { get; set; }
        public string FailIDPattern { get; set; }
        public short? MaxIdLength { get; set; }
        public bool FailTransIDFromSMS { get; set; }
        public bool BlankId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public string GetTransactionID(string OriginalTransactionID, string DatabasePrimaryKey, string RechargeStatus)
        {
            string result = OriginalTransactionID;
            string RandomTransactionID = string.Empty;
            string RandomString = string.Empty;

            try
            {
                if (IdPattern.IndexOf("RANDOM") > 0)
                {
                    //result += " > RandomFound ";
                    int randomNumberLength = StaticData.StringToInt(IdPattern.Substring(IdPattern.IndexOf("RANDOM") + 6, 1));
                    //result += " > Length " + randomNumberLength;
                    if (randomNumberLength > 0)
                    {
                        RandomTransactionID = PSFTCrypto.GetKey(DateTime.UtcNow.AddHours(5.5).ToString() + DatabasePrimaryKey).Substring(2, randomNumberLength);
                        //result += " > RandomTransactionID " + RandomTransactionID;
                        RandomString = IdPattern.Substring(IdPattern.IndexOf("RANDOM") - 1, 9);
                        //result += " > RandomString " + RandomString;
                    }
                }
                else
                {
                    //result += " > RandomNotFound ";
                }

                string tmp = IdPattern;
                tmp = (tmp.IndexOf("[TRXID]") > -1) ? tmp.Replace("[TRXID]", DatabasePrimaryKey) : tmp;
                tmp = (tmp.IndexOf("[ORIGTRXID]") > -1) ? tmp.Replace("[ORIGTRXID]", OriginalTransactionID) : tmp;
                tmp = (tmp.IndexOf(RandomString) > -1) ? tmp.Replace(RandomString, RandomTransactionID) : tmp;
                //result += " > FinalID - " + tmp;

                result = tmp;
            }
            catch (Exception ex)
            {
                result += "exRand: " + ex.Message;
            }

            //if (string.IsNullOrEmpty(OriginalTransactionID) && BlankID == 1)
            //{
            //    string tmp = om.IDPattern;
            //    tmp = (tmp.IndexOf("[TRXID]") > -1) ? tmp.Replace("[TRXID]", DatabasePrimaryKey) : tmp;
            //    tmp = (tmp.IndexOf("[ORIGTRXID]") > -1) ? tmp.Replace("[ORIGTRXID]", OriginalTransactionID) : tmp;
            //    tmp = (tmp.IndexOf(RandomString) > -1) ? tmp.Replace(RandomString, RandomTransactionID) : tmp;
            //    return tmp.Length > om.MaxIdLength ? tmp.Substring(0, Convert.ToInt32(om.MaxIdLength)) : tmp;
            //}

            return result;
        }
    }
}
