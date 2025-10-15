using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{
    public class EventResponse
    {
        public string location { get; set; }
        public int retryAfter { get; set; }
        public string EventId { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }
}
