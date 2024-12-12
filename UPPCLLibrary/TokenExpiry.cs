using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class TokenExpiry
    {
        public int Discom { get; set; }
        public int BillDetail { get; set; }
        public int BillPostWallet { get; set; }
        public int BillPostPayment { get; set; }
        public int BillPostStatusCheck { get; set; }
        public int Forcefail { get; set; }
        public int OTS { get; set; }

        public string Message { get; set; }
    }
}
