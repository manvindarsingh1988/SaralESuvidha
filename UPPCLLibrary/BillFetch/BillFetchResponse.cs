using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class BillFetchResponse
    {
        public Body Body { get; set; }
        public string FAULTSTRING { get; set; }
        public string FAULTCODE { get; set; }

        public BillFetchResponse()
        {
            Body = new Body();
        }
    }
}
