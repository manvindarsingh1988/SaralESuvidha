using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.ViewModel;

namespace SaralESuvidha.Models
{
    public class UppclBillDirect
    {
        //http://101.53.153.183:3500/?cust_no=5558196976

        //{"captcha_image":"6 + 9 =\n","captcha_result":15,"acc_detail":"Prabhat      Account :5558196976      Add :E3 65 H,SECTOR H,KANPUR ROAD,LUCKNOW,226012                  Your payment is pending for         Rs. 0         Last date is 17-11-2020","bill_amount":"Rs. 0","bill_date":"17-11-2020","success":true}

        public string captcha_image { get; set; }
        public int captcha_result { get; set; }
        public string acc_detail { get; set; }
        public string bill_amount { get; set; }
        public string bill_date { get; set; }
        public bool success { get; set; }
        public string msg { get; set; }
        public string AccountNumber { get; set; }
        public string ConsumerName { get; set; }
        public string Address { get; set; }
        public decimal PendingAmount { get; set; }
        public DateTime DueDate { get; set; }

        public void ReadAddressName()
        {
            try
            {
                Address = StaticData.GetRegExFirstMatch(acc_detail, "Add :(.*)Your payment").Trim();
                ConsumerName = StaticData.GetRegExFirstMatch(acc_detail, "(.*)Account :").Trim().ToUpper(); //acc_detail\":\"
                AccountNumber = StaticData.GetRegExFirstMatch(acc_detail, "Account :(\\d{0,25})").Trim();
                bill_amount = bill_amount.Replace("Rs.", "").Trim();
            }
            catch (Exception)
            {

            }
        }
    }
}
