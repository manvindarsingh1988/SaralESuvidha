using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.OTS
{
    public class AmountDetails
    {
        public AmountData Data { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

    public class AmountData
    {
        public string Status { get; set; }
        public string StatusDescr { get; set; }
        public string AccountId { get; set; }
        public List<Installment> InstallmentList1 { get; set; }
        public List<FullPayment> FullPaymentList { get; set; }
        public decimal CurrentBillAmount { get; set; }
        public decimal Payment31 { get; set; }
        public decimal LPSC31 { get; set; }
        public string TarrifType { get; set; }
        public string LoadUom { get; set; }
        public decimal SanctionLoad { get; set; }
        public string SupplyType { get; set; }
        public string ConsumerName { get; set; }
        public string AccountIdOut { get; set; }
        public decimal TotoalOutStandingAmount { get; set; }
        public string Message { get; set; }

    }

    public class Installment
    {
        public int NoOfInstallments { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal RegistrationAmount { get; set; }
        public decimal Downpayment { get; set; }
        public decimal LPSCWaivOff { get; set; }
        public string PaymentOption { get; set; }
        public decimal WaveOffPercent { get; set; }
    }

    public class FullPayment
    {
        public decimal Downpayment { get; set; }
        public decimal LPSCWaivOff { get; set; }
        public string PaymentOption { get; set; }
        public decimal WaveOffPercent { get; set; }
        public decimal RegistrationAmount { get; set; }
    }
}
