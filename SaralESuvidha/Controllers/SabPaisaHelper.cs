using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using SaralESuvidha.Services;
using SaralESuvidha.ViewModel;
using static QRCoder.PayloadGenerator;
using System.Threading.Tasks;
using System;
using SaralESuvidha.Models;

namespace SaralESuvidha.Controllers
{
    public class SabPaisaHelper
    {
        public static async Task<TransactionStatus> PostOrder(SabPaisaService sabPaisaService, string query, string requestIp, string requestMachine, int orderNo)
        {
            string result = string.Empty;
            try
            {
                // Optional: verify with status API
                var verified = await sabPaisaService.CheckStatusAsync(query);
                //var verified1 = await sabPaisaService.CheckStatusByJobAsync(verified.TxnId);
                RazorpayOrder razorpayOrder = StaticData.RazorpayOrderLoadByRazorpayId(verified.TxnId);
                var fee = verified.PaidAmount - verified.Amount;
                verified.Fee = fee;
                StaticData.RazorpayOrderUpdateFees(verified.TxnId, Convert.ToInt64(fee).ToString(), "", "", verified.Status);
                if (razorpayOrder.Amount == verified.Amount)
                {
                    RecordSaveResponse recordSaveResponse = StaticData.RazorpayOrderUpdateOPS(verified.TxnId, verified.SabPaisaTxnId, "");
                    if (recordSaveResponse.OperationMessage.Contains("Success") && verified.Status.ToUpper() == "SUCCESS")
                    {
                        RTran fundTransferRTran = new RTran();
                        try
                        {
                            string tranType = "cr";

                            fundTransferRTran.RequestIp = requestIp;
                            fundTransferRTran.RequestMachine = requestMachine;
                            fundTransferRTran.RetailUserOrderNo = orderNo;

                            fundTransferRTran.Amount = verified.Amount;


                            fundTransferRTran.Extra1 = "razor";
                            fundTransferRTran.Extra2 = verified.TxnId;

                            if (tranType == "cr")
                            {
                                fundTransferRTran.CreditAmount = fundTransferRTran.Amount;
                                fundTransferRTran.TranType = 11;
                            }

                            if (tranType == "dr")
                            {
                                fundTransferRTran.DebitAmount = fundTransferRTran.Amount;
                                fundTransferRTran.TranType = 12;
                            }

                            fundTransferRTran.Remarks =
                                "Wallet topup via Razorpay order-" + verified.TxnId + ", payment id-" + verified.SabPaisaTxnId + ", method-" + verified.PaymentMode;
                            //fundTransferRTran.Remarks = "Wallet topup of Rs. " + razorpayOrder.Amount.ToString() + " , fees-" + (rFee/100).ToString() + " via Razorpay order-" + o + ", payment id-" + p;
                            fundTransferRTran.RequestMessage = "WEBPORTAL";

                            if (fundTransferRTran.Amount > 0)
                            {
                                result = fundTransferRTran.TransferFundByData("admin");
                            }
                            else
                            {
                                result = "Errors: Invalid amount, can not process transfer.";
                            }
                        }
                        catch (Exception ex)
                        {
                            result = "Errors: Exception: " + ex.Message;
                        }
                        finally
                        {
                            fundTransferRTran = null;
                        }
                    }
                    else
                    {
                        result = verified.Status.ToUpper() != "SUCCESS" ? "Errors: Transaction Failed on provider end!" : "Errors: Invalid amount, can not process transfer.";
                    }
                }
                else
                {
                    result =
                        "Error: Unable to verify payment details, invalid amount received. Please contact Saral e-Suvidha custome support.";
                }
                verified.Message = verified.Message + " " + result;
                return verified;
            }
            catch (Exception ex)
            {
                result = "Error: Error in processing payments. " + ex.Message;
                return new TransactionStatus() { Status = "FAILED", Message = result };
            }
        }

        public static async Task<string> IntiateOrder(SabPaisaService sabPaisaService, string name, decimal amount, string email, string mobile, string url, string retailerId)
        {
            string result = string.Empty;
            if(StaticData.CheckTopupServiceIsDown("SabPaisa") == false)
            {
                return "Error: Please refresh the page and try again.";
            }
            try
            {
                if (amount > 0)
                {
                    decimal tempAmount = Convert.ToDecimal(amount.ToString("N2"));
                    //tempAmount = tempAmount * 100;
                    string amountWithPaisa = (tempAmount * 100).ToString("N0").Replace(",", "");
                    var orderResponse = StaticData.RazorpayOrderSave(name, tempAmount, amountWithPaisa, email, mobile, retailerId, "SabPaisa");
                    if (!string.IsNullOrEmpty(orderResponse.Id))
                    {
                        TransactionRequest request = new TransactionRequest()
                        {
                            TxnId = orderResponse.Id,
                            Amount = tempAmount,
                            ReturnUrl = url,
                            CustomerEmail = email,
                            CustomerName = name,
                            CustomerPhone = mobile,
                            Udf20 = retailerId
                        };
                        var redirectPage = await sabPaisaService.InitiatePaymentAsync(request);
                        RecordSaveResponse orderIdSaveResponse = StaticData.RazorpayOrderUpdateOrderId(orderResponse.Id, orderResponse.Id);
                        result = redirectPage;
                    }
                    else
                    {
                        result = "Error: " + orderResponse.OperationMessage;
                    }
                }
                else
                {
                    result = "Error: Invalid amount.";
                }
            }
            catch (Exception ex)
            {
                result = "Error: Can not generate order. " + ex.Message;
            }
            return result;
        }
    }
}
