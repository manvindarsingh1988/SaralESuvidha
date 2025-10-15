using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.EventResponseType.WalletTransfer
{
    public class WalletTransferResponse
    {
        public string createdAt { get; set; }
        public string date { get; set; }
        public string id { get; set; }
        public string modifiedAt { get; set; }
        public bool @new { get; set; }
        public Payload payload { get; set; }
        public List<Response> response { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string message { get; set; }

        public WalletTransferResponse()
        {
            payload= new Payload();
            response= new List<Response>();
        }
    }
}
