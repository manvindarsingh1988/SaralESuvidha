using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class ApiStatusCode
    {
        public string Id { get; set; }
        public string ApiId { get; set; }
        public string StatusCode { get; set; }
        public string StatusType { get; set; }
        /// <summary>
        /// 1=PROCESS
        /// 2=SUSPENSE/HOLD
        /// 3=SUCCESS
        /// 4=REFUND
        /// 10=FAIL
        /// </summary>
        public short? OurStatusType { get; set; }
        public string StatusDescription { get; set; }
        /// <summary>
        /// 1=InitialPush
        /// 2=StatusCheck
        /// 3=Callback
        /// </summary>
        public short? StatusSource { get; set; }
        public bool Active { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
