using SaralESuvidha.ViewModel;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace SaralESuvidha.Controllers
{
    public class KYCHelper
    {
        public static string SendOTP(string aadharId)
        {
            var tokenResult = Authenticate();
            var tokens = tokenResult.Item2;
            var token = tokenResult.Item1;
            var url = "https://api.sandbox.co.in/kyc/aadhaar/okyc/otp";
            var obj = new Addhar();
            obj.aadhaar_number = aadharId;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            json = json.Replace("entity", "@entity");
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var cl = new HttpClient();
            string contentType = "application/json";
            cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var requiredTokens = tokens.Where(_ => _.Name != "Authorization" && _.Name != "x-api-secret");
            foreach (var item in requiredTokens)
            {
                cl.DefaultRequestHeaders.Add(item.Name, item.Token);
            }
            cl.DefaultRequestHeaders.Add("Authorization", token);
            var result = cl.PostAsync(url, data).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            var validation = Newtonsoft.Json.JsonConvert.DeserializeObject<AadharValidation>(resultContent);
            return Newtonsoft.Json.JsonConvert.SerializeObject(validation.message != null ? validation : validation.data);
            //var obj = new AadharValidation() { data = new data { message = "OTP sent successfully", reference_id = "26538192" } };
            //return Newtonsoft.Json.JsonConvert.SerializeObject(obj.message != null ? obj : obj.data);
        }

        private static (string, List<KYCToken>) Authenticate()
        {
            var isExpired = false;
            var token = string.Empty;
            
            var tokens = StaticData.GetKYCTokens();
            var authorizeToken = tokens.FirstOrDefault(_ => _.Name == "Authorization");
            if (authorizeToken != null)
            {
                var timeDiff = (DateTime.Now - authorizeToken.CreatedOn).TotalMinutes;
                if (timeDiff > 1430)
                {
                    isExpired = true;
                }
                else
                {
                    token = authorizeToken.Token;
                }
            }
            if (authorizeToken == null || isExpired)
            {
                var createdOn = DateTime.Now;
                var url = "https://api.sandbox.co.in/authenticate";
                var cl = new HttpClient();
                string contentType = "application/json";
                cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                var requiredTokens = tokens.Where(_ => _.Name != "Authorization");
                foreach (var item in requiredTokens)
                {
                    cl.DefaultRequestHeaders.Add(item.Name, item.Token);
                }
                var res = cl.PostAsync(url, null).Result;
                token = res.Content.ReadAsStringAsync().Result;
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(token);
                token = obj.access_token;
                StaticData.InsertOrUpdateKYCTokens(token, createdOn);
            }
            return (token, tokens);
        }

        public static string VerifyOTP(string referenceId, string otp, string folderPath, string aadharId)
        {
            var tokenResult = Authenticate();
            var tokens = tokenResult.Item2;
            var token = tokenResult.Item1;
            var url = "https://api.sandbox.co.in/kyc/aadhaar/okyc/otp/verify";
            var obj = new OTPVerify();
            obj.reference_id = referenceId;
            obj.otp = otp;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            json = json.Replace("entity", "@entity");
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
            var cl = new HttpClient();
            string contentType = "application/json";
            cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var requiredTokens = tokens.Where(_ => _.Name != "Authorization" && _.Name != "x-api-secret");
            foreach (var item in requiredTokens)
            {
                cl.DefaultRequestHeaders.Add(item.Name, item.Token);
            }
            cl.DefaultRequestHeaders.Add("Authorization", token);
            var result = cl.PostAsync(url, data).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            var validation = Newtonsoft.Json.JsonConvert.DeserializeObject<AadharDetails>(resultContent);
            if (validation != null && validation.data != null && validation.data.message == "Aadhaar Card Exists")
            {
                validation.data.aadharId = aadharId;
                folderPath = Path.Combine(folderPath + validation.data.reference_id + "/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var user = Newtonsoft.Json.JsonConvert.SerializeObject(validation.data);
                File.WriteAllText(folderPath + "Aadhar.txt", user);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(validation.message != null ? validation : validation.data);
            //var obj = new AadharDetails() { message = "Aadhaar Card Exists" };
            //return Newtonsoft.Json.JsonConvert.SerializeObject(obj.message != null ? obj : obj.data);
        }

        public static string VerifyPan(string panNo, string name, string dob, string folderPath, string referenceId)
        {
            var tokenResult = Authenticate();
            var tokens = tokenResult.Item2;
            var token = tokenResult.Item1;
            var url = "https://api.sandbox.co.in/kyc/pan/verify";
            var obj = new PANVerify();
            obj.pan = panNo;
            obj.name_as_per_pan = name;
            obj.date_of_birth = dob;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            json = json.Replace("entity", "@entity");
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
            var cl = new HttpClient();
            string contentType = "application/json";
            cl.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            var requiredTokens = tokens.Where(_ => _.Name != "Authorization" && _.Name != "x-api-secret");
            foreach (var item in requiredTokens)
            {
                cl.DefaultRequestHeaders.Add(item.Name, item.Token);
            }
            cl.DefaultRequestHeaders.Add("Authorization", token);
            var result = cl.PostAsync(url, data).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            var validation = Newtonsoft.Json.JsonConvert.DeserializeObject<PANDetails>(resultContent);
            if (validation != null && validation.data != null && validation.data.status == "valid")
            {
                folderPath = Path.Combine(folderPath + referenceId + "/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var user = Newtonsoft.Json.JsonConvert.SerializeObject(validation.data);
                File.WriteAllText(folderPath + "PAN.txt", user);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(validation.message != null ? validation : validation.data);
            //var obj = new PANDetails() { data = new data { status = "valid" } };
            //return Newtonsoft.Json.JsonConvert.SerializeObject(obj.message != null ? obj : obj.data);
        }

        public static bool SaveFile(RetailUserViewModel retailUserViewModel, string folderPath, IFormFile formFile, string fileName)
        {
            if (formFile != null)
            {
                var name = string.Empty;
                using (var target = new MemoryStream())
                {
                    formFile.CopyTo(target);
                    fileName = fileName + Path.GetExtension(formFile.FileName);
                    name = fileName;
                    fileName = fileName = folderPath + fileName;
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }
                    System.IO.File.WriteAllBytes(fileName, target.ToArray());
                    return true;
                }
            }
            return false;
        }

        public static RetailUserViewModel LoadUser(string id, string folderPath)
        {
            var retailUserToUpdate = new RetailUserViewModel();
            try
            {
                folderPath = Path.Combine(folderPath + id + "/");
                var filePath = folderPath + "Aadhar.txt";

                if (File.Exists(filePath))
                {
                    var aadharInfo = System.IO.File.ReadAllText(filePath);
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<data>(aadharInfo);
                    var nameList = data.name.Split(' ');
                    if (nameList.Count() == 1)
                    {
                        retailUserToUpdate.FirstName = nameList[0];
                        retailUserToUpdate.MiddleName = ".";
                        retailUserToUpdate.LastName = ".";
                    }
                    else if (nameList.Count() == 2)
                    {
                        retailUserToUpdate.FirstName = nameList[0];
                        retailUserToUpdate.MiddleName = ".";
                        retailUserToUpdate.LastName = nameList[1];
                    }
                    else if (nameList.Count() > 2)
                    {
                        retailUserToUpdate.FirstName = nameList[0];
                        var middleName = string.Empty;
                        for (var i = 1; i < nameList.Length - 1; i++)
                        {
                            middleName += " " + nameList[i];
                        }
                        retailUserToUpdate.MiddleName = middleName;
                        retailUserToUpdate.LastName = nameList[nameList.Length - 1];
                    }
                    if (data.gender.ToLower() == "m")
                    {
                        retailUserToUpdate.Gender = "Male";
                    }
                    else if (data.gender.ToLower() == "f")
                    {
                        retailUserToUpdate.Gender = "Female";
                    }
                    else
                    {
                        retailUserToUpdate.Gender = "Transgender";
                    }

                    DateTime dob = DateTime.ParseExact(data.date_of_birth, "dd-MM-yyyy",
                                           System.Globalization.CultureInfo.InvariantCulture);
                    retailUserToUpdate.DateOfBirth = dob;
                    retailUserToUpdate.ParmanentAddress = data.full_address;
                    retailUserToUpdate.PinCode = data.address.pincode;
                    retailUserToUpdate.ReferenceId = id;
                }
            }
            catch(Exception ex)
            {
                retailUserToUpdate.Address = ex.Message;
            }
            return retailUserToUpdate;
        }
    }

    public class Addhar
    {
        public string @entity { get; set; } = "in.co.sandbox.kyc.aadhaar.okyc.otp.request";
        public string aadhaar_number { get; set; }
        public string consent { get; set; } = "y";
        public string reason { get; set; } = "For KYC";
    }

    public class OTPVerify
    {
        public string @entity { get; set; } = "in.co.sandbox.kyc.aadhaar.okyc.request";
        public string reference_id { get; set; }
        public string otp { get; set; } = "y";
    }

    public class PANVerify
    {
        public string @entity { get; set; } = "in.co.sandbox.kyc.pan_verification.request";
        public string pan { get; set; }
        public string name_as_per_pan { get; set; }
        public string date_of_birth { get; set; }
        public string consent { get; set; } = "y";
        public string reason { get; set; } = "For KYC";
    }

    public class Token
    {
        public string access_token { get; set; }
    }

    public class AadharValidation
    {
        public string message { get; set; }
        public data data { get; set; }
    }

    public class data
    {
        public string message { get; set; }
        public string reference_id { get; set; }
        public string status { get; set; }
        public string full_address { get; set; }
        public string date_of_birth { get; set; }
        public string gender { get; set; }
        public string name { get; set; }
        public string photo { get; set; }
        public address address { get; set; }
        public string pan { get; set; }
        public string category { get; set; }
        public string remarks { get; set; }
        public bool name_as_per_pan_match { get; set; }
        public bool date_of_birth_match { get; set; }
        public string aadhaar_seeding_status { get; set; }
        public string aadharId { get; set; }
        
    }

    public class address
    {
        public string country { get; set; }
        public string district { get; set; }
        public string house { get; set; }
        public string pincode { get; set; }
        public string state { get; set; }
    }

    public class AadharDetails
    {
        public string message { get; set; }
        public data data { get; set; }
    }

    public class PANDetails
    {
        public string message { get; set; }
        public data data { get; set; }
    }
}
