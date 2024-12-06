using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.BillFetch
{
    public class detail
    {
        public ErrorInfo ErrorInfo { get; set; }

        public detail()
        {
            ErrorInfo = new ErrorInfo();
        }
    }
}
