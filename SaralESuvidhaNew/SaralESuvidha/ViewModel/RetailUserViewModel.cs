using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SaralESuvidha.ViewModel
{
    public class RetailUserViewModel
    {
        public string Id { get; set; }
        public string MasterId { get; set; }
        public int? UserType { get; set; }
        public short MarginType { get; set; }

        [DisplayName("First Name"), Required(ErrorMessage = "Required")]
        public String FirstName { get; set; }

        [DisplayName("Middle name"), Required(ErrorMessage = "Required")]
        public String MiddleName { get; set; }

        [DisplayName("Last Name"), Required(ErrorMessage = "Required")]
        public String LastName { get; set; }

        [DisplayName("EMail Address")]
        public String EMail { get; set; }

        [DisplayName("Gender")]
        public string Gender { get; set; }

       
        [DisplayName("Date Of Birth"), Required(ErrorMessage = "Required")]
        public DateTime DateOfBirth { get; set; }

        [DisplayName("Mobile Number"), Required(ErrorMessage = "Required")] 
        public string Mobile { get; set; }


        [DisplayName("Parmanent Address")] 
        public string ParmanentAddress { get; set; }

        [DisplayName("Current Address"), Required(ErrorMessage = "Required")]
        public string Address { get; set; }

        [DisplayName("Counter Location"), Required(ErrorMessage = "Required")]
        public string CounterLocation { get; set; }

        [DisplayName("City"), Required(ErrorMessage = "Select City")]
        public string City { get; set; }

        [DisplayName("Pin Code"), Required(ErrorMessage = "(Required)")]
        [RegularExpression(@"^[0-9]{6,6}?$", ErrorMessage = "Enter valid Pin Code")]
        public string PinCode { get; set; }

        [DisplayName("State"), Required(ErrorMessage = "Select State")]
        public string StateName { get; set; }
        
        public string Password { get; set; }

        [DisplayName("Margin Percent"), Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-5]{0,1}(\.[0-9]{1,2})?$", ErrorMessage = "Enter valid margin %")]
        public float? Commission { get; set; }

        [DisplayName("Account Active Status")]
        public int Active { get; set; }
        public int OrderNo { get; set; }

        [DisplayName("")]
        public IFormFile Photo { get; set; }

        [DisplayName("")]
        public IFormFile Agreement { get; set; }

        [DisplayName("")]
        public IFormFile Affidavit { get; set; }

        [DisplayName("")]
        public IFormFile PoliceVerification { get; set; }

        public string OperationMessage { get; set; }

        public int DocVerification { get; set; }

        public int PhysicalKYCDone {  get; set; }
        public string ReferenceId {  get; set; }


        [DisplayName("DistributorType")]
        public string DistributorType { get; set; }

        public string FailureReason { get; set; }
        public string Discom { get; set; }

        public RetailUserViewModel Save()
        {
            Password = StaticData.GeneratePassword(8);
            using (var con = new SqlConnection(StaticData.conString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MasterId", MasterId);
                parameters.Add("@UserType", UserType);
                parameters.Add("@MarginType", MarginType);
                parameters.Add("@FirstName", FirstName);
                parameters.Add("@MiddleName", MiddleName);
                parameters.Add("@LastName", LastName);
                parameters.Add("@EMail", EMail);
                parameters.Add("@Gender", Gender);
                parameters.Add("@DateOfBirth", DateOfBirth);
                parameters.Add("@Mobile", Mobile);
                parameters.Add("@Address", Address);
                parameters.Add("@City", City);
                parameters.Add("@PinCode", PinCode);
                parameters.Add("@Country", "India");
                parameters.Add("@StateName", StateName);
                parameters.Add("@Password", Password);
                parameters.Add("@CreditLimit", 0);
                parameters.Add("@MinFundValue", 0);
                parameters.Add("@Commission", Commission);
                parameters.Add("@Active", Active);
                parameters.Add("@SignupDate", DateTime.Now);
                parameters.Add("@DistributorType", DistributorType);                
                parameters.Add("@ParmanentAddress", ParmanentAddress);                
                parameters.Add("@CounterLocation", CounterLocation);                
                parameters.Add("@Discom", Discom);                

                var saveResponse = con.QuerySingleOrDefault<dynamic>("usp_RetailUserInsert", parameters, commandType: System.Data.CommandType.StoredProcedure);
                OperationMessage = saveResponse.OperationMessage;
                Id = saveResponse.Id;
            }
            return this;
        }

        public RetailUserViewModel Update()
        {
            Password = StaticData.GeneratePassword(8);
            using (var con = new SqlConnection(StaticData.conString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", Id);
                parameters.Add("@Mobile", Mobile);
                parameters.Add("@Country", "India");
                parameters.Add("@StateName", StateName);
                parameters.Add("@City", City);
                parameters.Add("@PinCode", PinCode);
                parameters.Add("@Address", Address);
                parameters.Add("@CounterLocation", CounterLocation);
                parameters.Add("@DistributorType", DistributorType);
                parameters.Add("@Discom", Discom);

                OperationMessage = con.QuerySingleOrDefault<string>("usp_RetailUserUpdate", parameters, commandType: System.Data.CommandType.StoredProcedure);                
            }
            return this;
        }

    }
}
