using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class PendingRechargeData
    {
        public string InitialResponseData { get; set; }

        public string CallbackData { get; set; }

        public string ProviderName { get; set; }

        public string RechargeStatus { get; set; }

        public long? TimeDelay { get; set; }

        public string ClientId { get; set; }
    }
}
