using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class OperationResponse
    {
        public string RecordIdPrimary { get; set; }
        
        public string RecordIdSecondary { get; set; }
        public string OperationMessage { get; set; }
        public int RecordCount { get; set; }
     }
}
