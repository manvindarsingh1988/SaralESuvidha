﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary.OTS
{
    public class CheckEligibility
    {
        public Data Data { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

    public class Data
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }
}