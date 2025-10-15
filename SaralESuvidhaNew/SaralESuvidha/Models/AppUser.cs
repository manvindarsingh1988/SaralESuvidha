using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class AppUser
    {
        public String ID { get; set; }
        public String MasterID { get; set; }
        public int? UserType { get; set; }
        public String FirstName { get; set; }
        public String MiddleName { get; set; }
        public String LastName { get; set; }
        public String EMail { get; set; }
        public String Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public String Mobile { get; set; }
        public String Phone { get; set; }
        public String Address { get; set; }
        public String City { get; set; }
        public String PinCode { get; set; }
        public String Country { get; set; }
        public String Password { get; set; }
        public String SecurityQuestion { get; set; }
        public String SecurityAnswer { get; set; }
        public byte? Active { get; set; }
        public byte? OtpActive { get; set; }
        public String MemberLevel { get; set; }
        public String RequestIp { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? SignupDate { get; set; }
        public long OrderNo { get; set; }
    }
}
