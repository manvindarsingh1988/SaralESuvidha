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
        [Route("GetMappedUsers")]
        public List<UserInfo> GetMappedUsers()
        {
            return StaticData.GetMappedUsers();
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
        public LiabilityInfo GetLiabilityAmountByRetailerId(string userId, DateTime date)
        {
            return StaticData.GetLiabilityAmountByRetailerId(userId, date);
        }

        [HttpGet]
        [Route("GetLiabilityAmountByCollectorId")]
        public LiabilityInfo GetLiabilityAmountByCollectorId(string userId, DateTime date)
        {
            return StaticData.GetLiabilityAmountByCollectorId(userId, date);
        }

        [HttpGet]
        [Route("GetMappedCollectorsByRetailerId")]
        public List<MappedUserInfo> GetMappedCollectorsByRetailerId(string userId)
        {
            return StaticData.GetMappedCollectorsByRetailerId(userId);
        }

        [HttpGet]
        [Route("GetLiabilityAmountOfAllRetailers")]
        public List<LiabilityInfo> GetLiabilityAmountOfAllRetailers(DateTime date)
        {
            return StaticData.GetLiabilityAmountOfAllRetailers(date);
        }

        [HttpPost]
        [Route("AddLadgerInfo")]
        public BoolResult AddLadgerInfo(LadgerInfo ladger)
        {
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
        public List<Ladger> GetLadgerInfoByRetailerid(DateTime date, string retailerId)
        {
            return StaticData.GetLadgerInfoByRetailerid(date, retailerId);
        }

        [HttpGet]
        [Route("GetLadgerInfoByCollectorId")]
        public List<Ladger> GetLadgerInfoByCollectorId(DateTime date, string collectorId)
        {
            return StaticData.GetLadgerInfoByCollectorId(date, collectorId);
        }

        [HttpGet]
        [Route("GetLadgerInfoByRetaileridAndCollectorId")]
        public List<Ladger> GetLadgerInfoByRetaileridAndCollectorId(DateTime date, string retailerId, string collectorId)
        {
            return StaticData.GetLadgerInfoByRetaileridAndCollectorId(date, retailerId, collectorId);
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
        public List<CollectorLiabilityInfo> GetCollectorLiabilities(DateTime date)
        {
            return StaticData.GetCollectorLiabilities(date);
        }

        [HttpGet]
        [Route("GetCollectorLiabilityDetails")]
        public List<Ladger> GetCollectorLiabilityDetails(DateTime date, string collectorId)
        {
            return StaticData.GetCollectorLiabilityDetails(date, collectorId);
        }

        [HttpGet]
        [Route("GetPendingApprovalLedgers")]
        public List<Ladger> GetPendingApprovalLedgers()
        {
            return StaticData.GetPendingApprovalLedgers();
        }
    }

    public class CollectorRetailerMapping
    {
        public string CollectorId { get; set; }
        public string RetailerId { get; set; }
    }

    public class LiabilityInfo
    {
        public string RetailUserId { get; set; }
        public decimal Amt { get; set; }
        public decimal HandoverAmt { get; set; }
        public decimal ClearedAmt { get; set; }
        public decimal PreviousClosingAmt { get; set; }
        public string RetailUserName { get; set; }
        public string Status { get; set; }
    }

    public class CollectorLiabilityInfo
    {
        public string CollectorId { get; set; }
        public string CollectorUserName { get; set; }
        public decimal Amount { get; set; }
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
