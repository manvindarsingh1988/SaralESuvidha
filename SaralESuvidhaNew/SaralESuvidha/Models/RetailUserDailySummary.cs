using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace SaralESuvidha.Models
{
    public class RetailUserDailySummary
    {
        public int OrderNo { get; set; }
        public string RetailerName { get; set; }
        public string Master { get; set; }
        public string MasterId { get; set; }
        public string MasterOfMaster { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public Dictionary<DateTime, decimal?> DailyAmounts { get; set; } = new Dictionary<DateTime, decimal?>();
    }

    
}