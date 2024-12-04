using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class RetailUserGrid
    {
        public long USL { get; set; }
        public string Id { get; set; }
        public string UserTypeString { get; set; }
        public string RetailerName { get; set; }
        public string MobileNumber { get; set; }
        public string City { get; set; }
        public string EMail { get; set; }
        public string Parent { get; set; }
        public string ParentName { get; set; }
        public string Address { get; set; }
        public string CscId { get; set; }
        public int UserType { get; set; }
        public short MarginType { get; set; }
        public decimal Balance { get; set; }
        public string DefaultUtilityOperator { get; set; }
        public bool ApiEnabled { get; set; }
        public short Active { get; set; }
        public DateTime? LoginTime { get; set; }
        public short? DocVerification { get; set; }
        public short? PhysicalKYCDone { get; set; }
        public DateTime? ActivatedTill { get; set; }

        public string FailureReason { get; set; }
        public short? DocVerificationFailed { get; set; }
        public short? AgreementAccepted { get; set; }
    }
}
