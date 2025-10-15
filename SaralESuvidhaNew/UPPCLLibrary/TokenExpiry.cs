using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class TokenExpiry
    {
        public int EventDetail { get; set; }
        public int AgentCreation { get; set; }
        public int AgentStatusByMobile { get; set; }
        public int AgentActivate { get; set; }
        public int WalletTopup { get; set; }
        public int WalletTopupStatusByRange { get; set; }
        public int WalletBalance { get; set; }

        public string Message { get; set; }
    }
}
