using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.EventResponseType.WalletTransfer
{
    public class WalletTransferByDateRangeResponse
    {
        public int count { get; set; }
        public List<Item> items { get; set; }
        public string nextPageToken { get; set; }
        public string status { get; set; }
        public string message { get; set; }

        public WalletTransferByDateRangeResponse() { 
            items = new List<Item>();
        }
        
    }
}
