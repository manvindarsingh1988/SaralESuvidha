using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaralESuvidha.Models
{
    public class ApiResponseParsing
    {
        public string Id { get; set; }
        public string ApiId { get; set; }
        public string OperatorId { get; set; }
        public string ParsingName { get; set; }
        public string ValidResponseText { get; set; }
        public bool ValidResponseCheckEnabled { get; set; }
        public string InvalidResponseText { get; set; }
        public short Priority { get; set; }
        public string SuccessText { get; set; }
        public string FailText { get; set; }
        public string PendingText { get; set; }
        public string BalanceText { get; set; }
        public string RefundText { get; set; }
        public string SystemRecordIdParameterName { get; set; }
        public string ApiRecordIdParameterName { get; set; }
        public string ApiStatusParameterName { get; set; }
        public string ApiRematksParameterName { get; set; }
        public string ApiBalanceParameterName { get; set; }
        public string ApiMobileParameterName { get; set; }
        public string ApiAmountParameterName { get; set; }
        public string ApiOperatorIdparameterName { get; set; }
        public string ApiErrorCodeParameterName { get; set; }
        public string TextBeforeNumber { get; set; }
        public string TextAfterNumber { get; set; }
        public string TextBeforeAmount { get; set; }
        public string TextAfterAmount { get; set; }
        public string TextBeforeRetailerNumber { get; set; }
        public string TextAfterRetailerNumber { get; set; }
        public string TextBeforeParameter1 { get; set; }
        public string TextAfterParameter1 { get; set; }
        public string TextBeforeParameter2 { get; set; }
        public string TextAfterParameter2 { get; set; }
        public string TextBeforeParameter3 { get; set; }
        public string TextAfterParameter3 { get; set; }
        public string TextBeforeOurId { get; set; }
        public string TextAfterOurId { get; set; }
        public string TextBeforeLiveId { get; set; }
        public string TextAfterLiveId { get; set; }
        public string TextBeforeBalance { get; set; }
        public string TextAfterBalance { get; set; }
        public bool RegExEnabled { get; set; }
        public string BalanceRegex { get; set; }
        public string NumberRegex { get; set; }
        public string AmountRegex { get; set; }
        public string LiveIdRegex { get; set; }
        public string OurIdRegex { get; set; }
        public string RechargeStatusRegex { get; set; }
        public string OtherApiIdRegex { get; set; }
        public string ValidResponseRegex { get; set; }
        public string Parameter1Regex { get; set; }
        public string Parameter2Regex { get; set; }
        public string Parameter3Regex { get; set; }
        public string RetailerSimRegex { get; set; }
        public string RetailerSimOpenningBalanceRegex { get; set; }
        public string RetailerSimBalanceRegex { get; set; }
        public string PreBalanceRegex { get; set; }
        public string PostBalanceRegex { get; set; }
        public string StatusCodeInitialPushRegex { get; set; }
        public string StatusDescriptionInitialPushRegex { get; set; }
        public string StatusCodeStatusCheckRegex { get; set; }
        public string StatusDescriptionStatusCheckRegex { get; set; }
        public string OurIdStatusCheckRegex { get; set; }
        public string ApiIdStatusCheckRegex { get; set; }
        public string LiveIdStatusCheckRegex { get; set; }
        public string MobileNumberStatusCheckRegex { get; set; }
        public string AmountStatusCheckRegex { get; set; }
        public string RetailerSimStatusCheckRegex { get; set; }
        public string RetailerSimStatusCheckOpenningBalanceRegex { get; set; }
        public string RetailerSimStatusCheckBalanceRegex { get; set; }
        public string StatusCodeCallbackRegex { get; set; }
        public string StatusDescriptionCallbackRegex { get; set; }
        public string OurIdCallbackRegex { get; set; }
        public string ApiIdCallbackRegex { get; set; }
        public string LiveIdCallbackRegex { get; set; }
        public string MobileNumberCallbackRegex { get; set; }
        public string AmountCallbackRegex { get; set; }
        public string PrebalanceCallbackRegex { get; set; }
        public string PostBalanceCallbackRegex { get; set; }
        public string MarginPercentCallbackRegex { get; set; }
        public string MarginAmountCallbackRegex { get; set; }
        public string RetailerSimCallbackRegex { get; set; }
        public string RetailerSimOpenningBalanceCallbackRegex { get; set; }
        public string RetailerSimBalanceCallbackRegex { get; set; }
        public bool? ApiNameCallbackRegexEnabled { get; set; }
        public string ApiNameCallbackRegex { get; set; }
        public short? PaymentStatus { get; set; }
        public bool Active { get; set; }
    }
}
