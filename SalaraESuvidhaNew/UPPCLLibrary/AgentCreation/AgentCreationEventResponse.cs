using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.AgentCreation
{
    public class AgentCreationEventResponse
    {
        public string createdAt { get; set; }
        public string date { get; set; }
        public string id { get; set; }
        public string modifiedAt { get; set; }
        public bool @new { get; set; }
        public Payload payload { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public string type { get; set; }

        public AgentCreationEventResponse()
        {
            payload = new Payload();
        }
    }
}
