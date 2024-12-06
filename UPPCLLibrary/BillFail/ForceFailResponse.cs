using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UPPCLLibrary.BillFail
{
    public class ForceFailResponse
    {
        public List<string> fieldError { get; set; }

        
        public string message { get; set; }
        public int code { get; set; }
    }
}
