using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class OperatorMaster
    {
        public short ID { get; set; }
        public String OperatorName { get; set; }
        public String AccName { get; set; }
        public String OperatorType { get; set; }
        public String ValidationExpression { get; set; }
        public string ValidDenomination { get; set; }
        public String DetailedOperatorName { get; set; }
        public String AdditionalInfo1 { get; set; }
        public String AdditionalInfo1Value { get; set; }
        public String AdditionalInfo1Validation { get; set; }
        public String AdditionalInfo2 { get; set; }
        public String AdditionalInfo2Value { get; set; }
        public String AdditionalInfo2Validation { get; set; }
        public string ROfferServer { get; set; }
        public bool? Active { get; set; }
    }
}
