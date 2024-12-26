using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class ESuvidhaBillFetchResponse
    {
        public BillFetchResponse BillFetchResponse { get; set; }

        public bool NoBillDue { get; set; }
        public bool CanNotPay { get; set; }
        public bool TemporaryDisconnection { get; set; }
        public string Reason { get; set; }
        public string AddressLine { get; set; }
        public string DiscomDetails { get; set; }
        public string AmountDetails { get; set; }

        public int PayAmount { get; set; }
        public int MinimumPayAmount { get; set; }
        public int MaximumPayAmount { get; set; }
        public int DueDateRebate { get; set; }


        //If pay amount is less tha or equal to 100 then minimum pay amount is pay amount
        public ESuvidhaBillFetchResponse()
        {
            BillFetchResponse = new BillFetchResponse();
        }

        public void ValidateBill()
        {
            try
            {
                DueDateRebate = 0;

                if (!string.IsNullOrEmpty(BillFetchResponse.FAULTSTRING))
                {
                    Reason = BillFetchResponse.FAULTSTRING;
                    CanNotPay = true;
                    PayAmount = 0;
                    MinimumPayAmount = 0;
                    MaximumPayAmount = 0;
                }
                else if(!string.IsNullOrEmpty(BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail))
                {
                    string errorMessage = BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail.IndexOf("<ams:message>") > 0
                                            ? UPPCLManager.GetRegExFirstMatch(BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail,"<ams:message>(.*)</ams:message>")
                                            : BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail;
                    Reason = errorMessage;

                    if (BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail.IndexOf("faultstring") > 0)
                    {
                        Reason = UPPCLManager.GetRegExFirstMatch(BillFetchResponse.Body.Fault.detail.ErrorInfo.ErrorDetails.detail, "faultstring\":\"(.*)\",\"faultactor");
                    }

                    CanNotPay = true;
                    PayAmount = 0;
                    MinimumPayAmount = 0;
                    MaximumPayAmount = 0;
                }
                else
                {
                    decimal accountInfo = Convert.ToDecimal(BillFetchResponse.Body.PaymentDetailsResponse.AccountInfo);
                    if (accountInfo <= 0)
                    {
                        Reason = "No bill due.";
                        CanNotPay = true;
                        PayAmount = 0;
                        MinimumPayAmount = 0;
                        MaximumPayAmount = 0;
                    }
                    else if(accountInfo > 0)
                    {
                        Reason = "";//Bill details success.
                        CanNotPay = false;
                        PayAmount = (int) Math.Round(accountInfo, MidpointRounding.AwayFromZero);

                        //Checking for the due date rebate.
                        try
                        {
                            if (BillFetchResponse.Body.PaymentDetailsResponse.Param1 != null)
                            {
                                DueDateRebate = (int)Math.Round(Convert.ToDecimal(BillFetchResponse.Body.PaymentDetailsResponse.Param1.ToString()), MidpointRounding.ToPositiveInfinity);
                            }
                        }
                        catch (Exception exParam1)
                        {
                            Reason += "Exception param1: " + BillFetchResponse.Body.PaymentDetailsResponse.Param1 + " > " + exParam1.Message;
                        }

                        if (BillFetchResponse.Body.PaymentDetailsResponse.TDStatus == "False")
                        {
                            TemporaryDisconnection = false;
                        }
                        else if (BillFetchResponse.Body.PaymentDetailsResponse.TDStatus == "True")
                        {
                            TemporaryDisconnection = true;
                        }

                        if (TemporaryDisconnection)
                        {
                            MinimumPayAmount = (int) Math.Round((accountInfo * 25)/100, MidpointRounding.AwayFromZero);
                            
                        }
                        else if(PayAmount < 250 && BillFetchResponse.Body.PaymentDetailsResponse.LifelineAct == "Y")
                        {
                            MinimumPayAmount = PayAmount;
                        }
                        else if (PayAmount < 1000 && BillFetchResponse.Body.PaymentDetailsResponse.LifelineAct == "N")
                        {
                            MinimumPayAmount = PayAmount;
                        }
                        else if(BillFetchResponse.Body.PaymentDetailsResponse.LifelineAct == "Y")
                        {
                            var minAmount = (int)Math.Round((accountInfo * 10) / 100, MidpointRounding.AwayFromZero);
                            MinimumPayAmount = minAmount > 250 ? minAmount : 250;
                        }
                        else if (BillFetchResponse.Body.PaymentDetailsResponse.LifelineAct == "N")
                        {
                            var minAmount = (int)Math.Round((accountInfo * 25) / 100, MidpointRounding.AwayFromZero);
                            MinimumPayAmount = minAmount > 1000 ? minAmount : 1000;
                        }

                        if (PayAmount < 250)
                        {
                            MinimumPayAmount = PayAmount;
                        }

                        MaximumPayAmount = PayAmount;

                        try
                        {
                            string tdStatus = TemporaryDisconnection ? "<span class='text-danger'>Disconnected</span>" : "<span class='text-success'>Connected</span>";
                            //string tdStatus = TemporaryDisconnection ? "Disconnected" : "Connected";
                            AddressLine = BillFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.AddressLine1 + "," + BillFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.AddressLine2 + ", " + 
                                          BillFetchResponse.Body.PaymentDetailsResponse.PremiseAddress.City + ", Mobile Number: " + BillFetchResponse.Body.PaymentDetailsResponse.MobileNumber;
                            DiscomDetails = "Division:" + BillFetchResponse.Body.PaymentDetailsResponse.Division + ", Sub Division:" + BillFetchResponse.Body.PaymentDetailsResponse.SubDivision + " - " + BillFetchResponse.Body.PaymentDetailsResponse.PurposeOfSupply + ", Area:" + BillFetchResponse.Body.PaymentDetailsResponse.ProjectArea;
                            AmountDetails = "Bill Id:" + BillFetchResponse.Body.PaymentDetailsResponse.BillID + ", Min Amount:" + MinimumPayAmount.ToString() + ", Connection Status:" + tdStatus;
                        }
                        catch (Exception exInner)
                        {

                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                Reason = "Exception: Validation: " + ex.Message;

            }
        }
    }
}
