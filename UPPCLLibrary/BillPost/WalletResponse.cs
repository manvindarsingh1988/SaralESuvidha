using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillPost
{
    public class WalletResponse
    {
        public double balance { get; set; }
        public string walletType { get; set; }
        public string walletStatus { get; set; }
        public bool blockchainCommit { get; set; }
        public string vanId { get; set; }
        public string message { get; set; } //"message": "No wallet exist with VAN: UPCAXXXXXXXXX "


    }
}
