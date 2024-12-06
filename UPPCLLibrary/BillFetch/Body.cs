using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class Body
    {
        public PaymentDetailsResponse PaymentDetailsResponse { get; set; }
        public Fault Fault { get; set; }

        public Body()
        {
            PaymentDetailsResponse = new PaymentDetailsResponse();
            Fault = new Fault();
        }
    }
}
