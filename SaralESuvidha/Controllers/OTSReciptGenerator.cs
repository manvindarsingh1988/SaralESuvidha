using Newtonsoft.Json;
using QRCoder;
using SaralESuvidha.ViewModel;
using System;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using UPPCLLibrary.OTS;

namespace SaralESuvidha.Controllers
{
    public class OTSReciptGenerator
    {
        public static UPPCLOTSReciptModal GenerateOTSRecipt(string accountId, string amount, int isFull, string reciptId, string rechargeStatus = null)
        {
            var obj = JsonConvert.DeserializeObject<CaseInitResponse>(StaticData.GetApiResponseByApiTypeAndConsumerId(accountId, "OTS_CaseInit", reciptId));
            var obj1 = JsonConvert.DeserializeObject<AmountDetails>(StaticData.GetApiResponseByApiTypeAndConsumerId(accountId, "OTS_AmountDetails", reciptId));
            decimal downPayment;
            if (isFull == 1)
            {
                var diff = Convert.ToDecimal(amount) - obj1.Data.FullPaymentList[0].RegistrationAmount;
                downPayment = obj1.Data.FullPaymentList[0].Downpayment - diff;
                if (downPayment < 0)
                {
                    downPayment = 0;
                }
            }
            else
            {
                downPayment = obj1.Data.InstallmentList1[0].Downpayment;
            }
            if (Decimal.TryParse(obj.Data.BillDetails.SanctionedLoadInKW, out decimal load))
            {
                obj.Data.BillDetails.SanctionedLoadInKW = load.ToString("0.00");
            }
            
            

            if (obj.Data.BillDetails.PurposeOfSupply == "LMV1")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (घरेलू)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV2")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (वाणिज्य)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV4")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (निजी संस्थान)";
            }
            if (obj.Data.BillDetails.PurposeOfSupply == "LMV6")
            {
                obj.Data.BillDetails.PurposeOfSupply += " (औद्योगिक)";
            }
            var modal = new UPPCLOTSReciptModal();
            modal.PurposeOfSupply = obj.Data.BillDetails.PurposeOfSupply;
            modal.downPayment = downPayment;
            modal.SanctionedLoadInKW = obj.Data.BillDetails.SanctionedLoadInKW + " KW";
            
            try
            {
                var hex = StaticData.ConvertStringToHex(reciptId);
                string verifyUrl = "http://saralesuvidha.com/Home/ReceiptOTSUPPCL?t=" + hex;//VerifyReceipt
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                QRCoder.Base64QRCode qr = new Base64QRCode();
                qr.SetQRCodeData(QrCodeInfo);
                modal.QrCode = "data:image/png;base64," + qr.GetGraphic(20);
            }
            catch (Exception)
            {

            }

            modal.RechargeMobileNumber = obj.Data.BillDetails.KNumber;
            modal.TelecomOperatorName = obj.Data.BillDetails.Discom;
            modal.ApiOperatorCode = obj.Data.BillDetails.Discom.Split('-')[0].Trim();

            modal.InfoTable = "<table class=tableData>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "श्रीमती/श्री";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj.Data.BillDetails.ConsumerName;
            modal.ConsumerName = obj.Data.BillDetails.ConsumerName;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "अकाउंट आईडी";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj.Data.BillDetails.KNumber;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "विधा";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj.Data.BillDetails.PurposeOfSupply;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "स्वीकृत भार";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj.Data.BillDetails.SanctionedLoadInKW + " KW";
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            if (isFull == 1)
            {
                modal.Para1 = $"आपके द्वारा OTS योजना के अंतर्गत दिनांक {DateTime.Now.ToString("dd-MM-yyyy")} को पंजीकरण किया गया है। इस योजना के अंतर्गत अधिकतम {obj1.Data.FullPaymentList[0].LPSCWaivOff} की छूट प्राप्त करने के लिये शेष बकाया धनराशि रू. {downPayment} का भुगतान दिनांक {DateTime.Now.AddDays(30).ToString("dd-MM-yyyy")} तक विभागीय खण्ड/उपखण्ड कार्यालय/कैश काउन्टर, जनसेवा केन्द्र, विद्युत सखी, फिनटेक प्रतिनिधि अथवा मीटर रीडर (बिलिगं एजेन्सी) अथवा UPPCL वेबसाइट (uppcl.org) के माध्यम से किया जा सकेगा";

                modal.InfoTable += "<tr>";
                modal.InfoTable += "<td>";
                modal.InfoTable += "चयनित विकल्प";
                modal.InfoTable += "</td>";
                modal.InfoTable += "<td>";
                modal.InfoTable += "एकमुश्त";
                modal.ChoosenOption = "एकमुश्त";
                modal.InfoTable += "</td>";
                modal.InfoTable += "</tr>";

            }
            else
            {
                modal.Para1 = $"आपके द्वारा OTS योजना के अंतर्गत दिनांक {DateTime.Now.ToString("dd-MM-yyyy")} को पंजीकरण किया गया है। इस योजना के अंतर्गत अधिकतम {obj1.Data.InstallmentList1[0].LPSCWaivOff} की छूट प्राप्त करने के लिये शेष बकाया धनराशि रू. {downPayment} का भुगतान निम्नांकित किश्तों में अपने मासिक विद्युत बिल के साथ विभागीय खण्ड/उपखण्ड कार्यालय/कैश काउन्टर, जनसेवा केन्द्र, विद्युत सखी, फिनटेक प्रतिनिधि अथवा मीटर रीडर (बिलिगं एजेन्सी) अथवा UPPCL वेबसाइट (uppcl.org) के माध्यम से किया जा सकेगा";
                modal.InfoTable += "<tr>";
                modal.InfoTable += "<td>";
                modal.InfoTable += "चयनित विकल्प";
                modal.InfoTable += "</td>";
                modal.InfoTable += "<td>";
                modal.InfoTable += "किश्त";
                modal.ChoosenOption = "किश्त";
                modal.InfoTable += "</td>";
                modal.InfoTable += "</tr>";

                modal.InstallmentTable = "<table class=tableData>";

                var months = new string[12]
                {
                    "जनवरी 2025",
                    "फ़रवरी 2025",
                    "मार्च 2025",
                    "अप्रैल 2025",
                    "मई 2025",
                    "जून 2025",
                    "जुलाई 2025",
                    "अगस्त 2025",
                    "सितम्बर 2025",
                    "अक्टूबर 2025",
                    "नवंबर 2025",
                    "दिसंबर 2025"
                };
                var installmet = new string[12]
                {
                    "पहली",
                    "दूसरी",
                    "तीसरी",
                    "चौथी",
                    "पांचवी",
                    "छटवी",
                    "सातवी",
                    "आठवी",
                    "नवमी",
                    "दसवी",
                    "ग्यारवी",
                    "बारहवीं"
                };
                modal.InstallmentTable += "<tr>";
                modal.InstallmentTable += "<td>";
                modal.InstallmentTable += "क्र.";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "<td>";
                modal.InstallmentTable += "किश्त धनराशि (रू.)";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "<td>";
                modal.InstallmentTable += "माह";
                modal.InstallmentTable += "</td>";
                modal.InstallmentTable += "</tr>";
                for (int i = 0; i < obj1.Data.InstallmentList1[0].NoOfInstallments; i++)
                {
                    if (i < 12)
                    {
                        modal.InstallmentTable += "<tr>";
                        modal.InstallmentTable += "<td>";
                        modal.InstallmentTable += installmet[i] + " किश्त";
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "<td>";
                        modal.InstallmentTable += obj1.Data.InstallmentList1[0].InstallmentAmount;
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "<td>";
                        modal.InstallmentTable += months[i];
                        modal.InstallmentTable += "</td>";
                        modal.InstallmentTable += "</tr>";
                    }
                }
                modal.InstallmentTable += "</table>";
            }
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "मूल बकाया";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj1.Data.Payment31;
            modal.Payment31_Mulbakaya = obj1.Data.Payment31;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "ब्याज";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += obj1.Data.LPSC31;
            modal.LPSC31_Byaj = obj1.Data.LPSC31;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "माफ़ी योग्य अधिकतम ब्याज";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += isFull == 1 ? obj1.Data.FullPaymentList[0].LPSCWaivOff : obj1.Data.InstallmentList1[0].LPSCWaivOff;
            modal.LPSCWaivOff_MafiYogyaAdhikatamByaj = isFull == 1 ? obj1.Data.FullPaymentList[0].LPSCWaivOff : obj1.Data.InstallmentList1[0].LPSCWaivOff;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "पंजीकरण धनराशि प्राप्त";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += isFull == 1 ? amount : obj1.Data.InstallmentList1[0].RegistrationAmount;
            modal.RegistrationAmount_PanjikaranRashi = isFull == 1 ? Convert.ToDecimal(amount) : obj1.Data.InstallmentList1[0].RegistrationAmount;
            
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "<tr>";
            modal.InfoTable += "<td>";
            modal.InfoTable += "Recharge Status";
            modal.InfoTable += "</td>";
            modal.InfoTable += "<td>";
            modal.InfoTable += rechargeStatus == null ? "Success" : rechargeStatus;
            modal.RechargeStatus = rechargeStatus == null ? "Success" : rechargeStatus;
            modal.InfoTable += "</td>";
            modal.InfoTable += "</tr>";
            modal.InfoTable += "</table>";
            return modal;
        }
    }
}
