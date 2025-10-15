using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace SaralESuvidha.Models
{
    public class MasterData
    {
        public List<Status> WorkFlows { get; set; }
        public List<Status> TransactionTypes { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
