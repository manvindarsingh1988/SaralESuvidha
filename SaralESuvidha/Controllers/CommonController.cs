using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SaralESuvidha.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace SaralESuvidha.Controllers
{
    public class CommonController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CommonController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult AmountToInr(double amount)
        {
            return Content(StaticData.AmountToInr(amount, false));
        }

        public IActionResult ActiveOperatorList(string o)
        {
            return Content(StaticData.OperatorUtilityList(o));
        }
        
        public IActionResult SmsPass(string m, string s)
        {
            try
            {
                return Content(StaticData.SmsPass(m));

            }
            catch (Exception ex)
            {
                return Content("Errors: Exception: " + ex.Message);
            }
        }

        public IActionResult DataTableToExcel(DataTable dataTableExcel, string fileNamePrefix)
        {
            string result = "";
            try
            {
                string GuidValue = fileNamePrefix + "_" + Guid.NewGuid().ToString();
                string path = "";
                path = Path.Combine(_hostingEnvironment.WebRootPath, "FileData", GuidValue + ".xlsx");
                result = path;
                FileInfo fi = new FileInfo(path);

                List<RTranReport> tranList = StaticData.RechargeReportListRetailClientByDate(12, DateTime.Now.AddMonths(-2), DateTime.Now);

                using (var package = new ExcelPackage(fi))
                {
                    var sheet = package.Workbook.Worksheets.Add("filedata");
                    //sheet.Cells["A1:A25"].Style.Numberformat.Format = "dd-MM-yyyy HH:mm";
                    sheet.Cells["A3"].LoadFromCollection(tranList);

                    for (int col = 1; col <= sheet.Dimension.End.Column; col++)
                    {
                        String columnLetter = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(col); // A
                        //sheet.Cells[columnLetter + "2"].Value = ? "" : "";
                        if (sheet.Cells[columnLetter + "3"].Value is DateTime)
                        {
                            sheet.Cells[columnLetter + "2"].Value = "Date";
                            sheet.Cells[columnLetter + "3:" + columnLetter + (tranList.Count + 3).ToString()].Style.Numberformat.Format = "dd-MM-yyyy HH:mm:ss";
                        }

                    }

                    int i = 1;
                    foreach (var prop in tranList[0].GetType().GetProperties())
                    {

                        String columnLetter = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(i); // A
                        sheet.Cells[columnLetter + "1"].Value = prop.Name;
                        //propertyList.Add(prop.Name);
                        i++;
                    }

                    sheet.Cells.AutoFitColumns();

                    package.Save();
                }
                result = "<a target='_blank' href=" + "" + "'/FileData/" + GuidValue + ".xlsx'" + " title='Download Excel File'>Download excel file.</a>";
            }
            catch (Exception ex)
            {
                result += "Exception: " + ex.Message;
            }
            finally
            {

            }
            return Content(result);
        }

        public IActionResult SendAadharOTP(string aadharId)
        {
            string result = string.Empty;
            result = KYCHelper.SendOTP(aadharId);
            return Content(result);
        }

        public IActionResult VerifyAadharOTP(string referenceId, string otp, string aadharId)
        {
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/");
            string result = string.Empty;
            result = KYCHelper.VerifyOTP(referenceId, otp, folderPath, aadharId);
            return Content(result);
        }

        public IActionResult VerifyPAN(string pan, string name, string dob, string referenceId)
        {
            string folderPath = Path.Combine(_hostingEnvironment.WebRootPath, "KYCDocFiles/");
            string result = string.Empty;
            result = KYCHelper.VerifyPan(pan, name, dob, folderPath, referenceId);
            return Content(result);
        }

    }
}
