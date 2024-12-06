using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class ErrorInfo
    {
        public string ErrorType { get; set; }
        public string ErrorDate { get; set; }
        public ErrorDetails ErrorDetails { get; set; }

        public ErrorInfo()
        {
            ErrorDetails = new ErrorDetails();
        }
    }
}
