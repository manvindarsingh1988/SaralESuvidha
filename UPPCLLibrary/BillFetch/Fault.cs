using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class Fault
    {
        public string faultcode { get; set; }
        public string faultstring { get; set; }
        public string faultactor { get; set; }
        public detail detail { get; set; }

        public Fault()
        {
            detail = new detail();
        }
    }
}
