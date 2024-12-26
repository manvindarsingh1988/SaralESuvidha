using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.ViewModel
{
    public class MarginSheet
    {
        public long ID { get; set; }
        public string RetailUserid { get; set; }
        public string RetailUserName { get; set; }
        public string UtilityType { get; set; }

        public string OperatorName { get; set; }

        public string OperatorCircle { get; set; }
        public decimal? FixMargin { get; set; }

        public float? MarginPercent { get; set; }
        public float? MarginPercentUpto200 { get; set; }

        public decimal? MaxMargin { get; set; }
        public decimal? MaxDailyLimit { get; set; }
        public bool? Active { get; set; }       

    }
}
