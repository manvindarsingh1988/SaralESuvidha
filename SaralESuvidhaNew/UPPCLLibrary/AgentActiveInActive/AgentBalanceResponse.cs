using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentActiveInActive
{
    public class AgentBalanceResponse
    {
        public double balance { get; set; }
        public bool blockchainCommit { get; set; }
        public bool synched { get; set; }
        public string vanId { get; set; }
        public string walletStatus { get; set; }
        public string walletType { get; set; }
        public string message { get; set; }

        //{
        //"balance": 0.0,
        //"blockchainCommit": false,
        //"synched": false,
        //"vanId": "UPCA",
        //"walletStatus": "ACTIVE",
        //"walletType": "AGENT"
        //}

    }
}
