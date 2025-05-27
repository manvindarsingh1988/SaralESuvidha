using Microsoft.AspNetCore.Mvc;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;

namespace SaralESuvidha.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashFlowController : ControllerBase
    {
        [HttpPost]
        [Route("SaveUser")]
        public RetailUserViewModel SaveUser(RetailUserViewModel retailUserViewModel)
        {
            retailUserViewModel.MasterId = null;
            retailUserViewModel.Password = StaticData.GeneratePassword(8);
            retailUserViewModel.Save();
            StaticData.UpdateKYCState(retailUserViewModel.Id, 1, 1, 0, string.Empty);
            return retailUserViewModel;
        }

        [HttpGet]
        [Route("GetRetailerUsers")]
        public List<UserInfo> GetRetailerUsers()
        {
            return StaticData.GetUsersByUserType(5);
        }

        [HttpGet]
        [Route("GetCollectorUsers")]
        public List<UserInfo> GetCollectorUsers()
        {
            return StaticData.GetUsersByUserType(12);
        }

        [HttpGet]
        [Route("GetCashierUsers")]
        public List<UserInfo> GetCashierUsers()
        {
            return StaticData.GetUsersByUserType(13);
        }

        [HttpGet]
        [Route("GetMappedUsersByCollectorId")]
        public List<MappedUserInfo> GetMappedUsersByCollectorId(string userId)
        {
            return StaticData.GetMappedUsersByCollectorId(userId);
        }

        [HttpGet]
        [Route("Login")]
        public UserInfo Login(string userId, string password)
        {
            return StaticData.CashFlowLogin(userId, password);
        }

        [HttpGet]
        [Route("GetMasterData")]
        public MasterData GetMasterData()
        {
            return StaticData.GetMasterData();
        }

        [HttpPost]
        [Route("AlignCollectorWithRetailerUser")]
        public StringResult AlignCollectorWithRetailerUser(CollectorRetailerMapping mapping)
        {
            return new StringResult { Response = StaticData.AlignCollectorWithRetailerUser(mapping.CollectorId, mapping.RetailerId) };
        }

        [HttpGet]
        [Route("GetLiabilityAmountByRetailerId")]
        public LiabilityInfo GetLiabilityAmountByRetailerId(string userId)
        {
            return StaticData.GetLiabilityAmountByRetailerId(userId);
        }

        [HttpGet]
        [Route("GetLiabilityAmountByCollectorId")]
        public LiabilityInfo GetLiabilityAmountByCollectorId(string userId)
        {
            return StaticData.GetLiabilityAmountByCollectorId(userId);
        }

        [HttpGet]
        [Route("GetMappedCollectorsByRetailerId")]
        public List<MappedUserInfo> GetMappedCollectorsByRetailerId(string userId)
        {
            return StaticData.GetMappedCollectorsByRetailerId(userId);
        }

        [HttpGet]
        [Route("GetLiabilityAmountOfAllRetailers")]
        public List<LiabilityInfo> GetLiabilityAmountOfAllRetailers()
        {
            return StaticData.GetLiabilityAmountOfAllRetailers();
        }

        [HttpPost]
        [Route("AddLadgerInfo")]
        public BoolResult AddLadgerInfo(LadgerInfo ladger)
        {
            ladger.GivenOn = DateTime.Now;
            return new BoolResult { Response = StaticData.AddLadgerInfo(ladger) };
        }

        [HttpPost]
        [Route("UpdateLadgerInfo")]
        public BoolResult UpdateLadgerInfo(LadgerInfo ladger)
        {
            return new BoolResult { Response = StaticData.UpdateLadgerInfo(ladger) };
        }

        [HttpGet]
        [Route("GetLadgerInfoByRetailerid")]
        public List<Ladger> GetLadgerInfoByRetailerid(bool all, string retailerId)
        {
            return StaticData.GetLadgerInfoByRetailerid(all, retailerId);
        }

        [HttpGet]
        [Route("GetLadgerInfoByCollectorId")]
        public List<Ladger> GetLadgerInfoByCollectorId(bool all, string collectorId)
        {
            return StaticData.GetLadgerInfoByCollectorId(all, collectorId);
        }

        [HttpGet]
        [Route("GetLadgerInfoByRetaileridAndCollectorId")]
        public List<Ladger> GetLadgerInfoByRetaileridAndCollectorId(bool all, string retailerId, string collectorId)
        {
            return StaticData.GetLadgerInfoByRetaileridAndCollectorId(all, retailerId, collectorId);
        }

        [HttpGet]
        [Route("GetLadgerInfosCreatedByCollectors")]
        public List<Ladger> GetLadgerInfosCreatedByCollectors(DateTime date)
        {
            return StaticData.GetLadgerInfosCreatedByCollectors(date);
        }

        [HttpDelete]
        [Route("DeleteLadgerInfo")]
        public StringResult DeleteLadgerInfo(int id)
        {
            return new StringResult { Response = StaticData.DeleteLadgerInfo(id) };
        }

        [HttpGet]
        [Route("GetCollectorLiabilities")]
        public List<LiabilityInfo> GetCollectorLiabilities()
        {
            return StaticData.GetCollectorLiabilities();
        }

        [HttpGet]
        [Route("GetCollectorLiabilityDetails")]
        public List<Ladger> GetCollectorLiabilityDetails(string collectorId)
        {
            return StaticData.GetCollectorLiabilityDetails(collectorId);
        }

        [HttpGet]
        [Route("GetCollectorLedgerDetails")]
        public List<Ladger> GetCollectorLedgerDetails(string collectorId)
        {
            return StaticData.GetCollectorLedgerDetails(collectorId);
        }

        [HttpGet]
        [Route("GetPendingApprovalLedgers")]
        public List<Ladger> GetPendingApprovalLedgers()
        {
            return StaticData.GetPendingApprovalLedgers();
        }

        [HttpGet]
        [Route("GetUserExtendedInfo")]
        public List<UserEx> GetUserExtendedInfo()
        {
            return StaticData.GetUserExtendedInfo();
        }

        [HttpGet]
        [Route("GetLinkedCollectors")]
        public List<CollectorInfo> GetLinkedCollectors(string userId)
        {
            return StaticData.GetLinkedCollectors(userId);
        }

        [HttpPost]
        [Route("UpdateIsSelfSubmitterFlag")]
        public BoolResult UpdateIsSelfSubmitterFlag(SubmitterFlagData data)
        {
            return new BoolResult { Response = StaticData.UpdateIsSelfSubmitterFlag(data) };
        }

        [HttpPost]
        [Route("UpdateIsThirdPartyFlag")]
        public BoolResult UpdateIsThirdPartyFlag(ThirdpartyFlagData data)
        {
            return new BoolResult { Response = StaticData.UpdateIsThirdPartyFlag(data) };
        }

        [HttpPost]
        [Route("UpdateOpeningBalanceData")]
        public BoolResult UpdateOpeningBalanceData(OpeningBalanceData data)
        {
            return new BoolResult { Response = StaticData.UpdateOpeningBalanceData(data) };
        }

        [HttpPost]
        [Route("LinkAllRetailersToNewCollector")]
        public BoolResult LinkAllRetailersToNewCollector(LinkingInfo data)
        {
            return new BoolResult { Response = StaticData.LinkAllRetailesToNewCollector(data) };
        }

        [HttpGet]
        [Route("GetLiabilityAmountOfAllRetailers")]
        public List<LiabilityInfo> GetLiabilityAmountOfAllRetailersByCollectorId(string collectorId)
        {
            return StaticData.GetLiabilityAmountOfAllRetailersByCollectorId(collectorId);
        }
        
        [HttpGet]
        [Route("GetPendingApprovalLedgersByCollectorId")]
        public List<Ladger> GetPendingApprovalLedgersByCollectorId(string collectorId)
        {
            return StaticData.GetPendingApprovalLedgersByCollectorId(collectorId);
        }

        [HttpGet]
        [Route("GetPassword")]
        public StringResult GetPassword(string userId)
        {
            return new StringResult { Response = StaticData.GetPassword(userId) };
        }

        [HttpPost]
        [Route("DeleteLinking")]
        public StringResult DeleteLinking(CollectorRetailerMapping data)
        {
            return new StringResult { Response = StaticData.DeleteLinking(data) };
        }
    }

    public class LinkingInfo
    {
        public string FromCollectorId { get; set; }
        public string ToCollectorId { get; set; }
    }

    public class OpeningBalanceData
    {
        public string UserId { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime OpeningBalanceDate { get; set; }
    }

    public class ThirdpartyFlagData
    {
        public string UserId { get; set; }
        public bool IsThirdParty { get; set; }
    }

    public class SubmitterFlagData
    {
        public string UserId { get; set; }
        public bool IsSelfSubmitter { get; set; }
    }

    public class CollectorInfo
    {
        public string CollectorUserId { get; set; }
        public string CollectorUser { get; set; }
    }

    public class UserEx
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public bool IsThirdParty { get; set; }
        public bool IsSelfSubmitter { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime OpeningBalanceDate { get; set; }
        public int UserType { get; set; }
        public string UserName { get; set; }
    }

    public class CollectorRetailerMapping
    {
        public string CollectorId { get; set; }
        public string RetailerId { get; set; }
    }

    public class LiabilityInfo
    {
        public string UserId { get; set; }
        public decimal LaibilityAmount { get; set; }
        public decimal ProjectionAmount { get; set; }
        public decimal RejectedAmount { get; set; }
        public decimal PendingApprovalAmount { get; set; }
        public decimal RetailerInitiatedAmount { get; set; }
        public decimal ClosingAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string UserName { get; set; }
    }

    public class LadgerInfo
    {
        public int Id { get; set; }
        public string RetailerId { get; set; }
        public string CollectorId { get; set; }
        public decimal Amount { get; set; }
        public int TransactionType { get; set; }
        public int WorkFlow { get; set; }
        public DateTime Date { get; set; }
        public DateTime GivenOn { get; set; }
        public string Comment { get; set; }
        public string CashierId { get; set; }
    }


    public class Ladger
    {
        public int Id { get; set; }
        public string RetailerId { get; set; }
        public string RetailerName { get; set; }
        public string CollectorId { get; set; }
        public string CollectorName { get; set; }
        public decimal Amount { get; set; }
        public int TransactionType { get; set; }
        public int WorkFlow { get; set; }
        public DateTime Date { get; set; }
        public DateTime GivenOn { get; set; }
        public string Comment { get; set; }
        public string CashierId { get; set; }
        public string CashierName { get; set; }
    }

    public class StringResult
    {
        public string Response { get; set; }
    }

    public class BoolResult
    {
        public bool Response { get; set; }
    }
}
