using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class AppUserLogin
    {
        public String Id { get; set; }
        public String MasterID { get; set; }
        public int UserType { get; set; }
        
        public String UserName { get; set; }
        public String EMail { get; set; }
        public String Mobile { get; set; }
        
        public byte Active { get; set; }
        public byte OtpActive { get; set; }

        public long OrderNo { get; set; }
    }
}
