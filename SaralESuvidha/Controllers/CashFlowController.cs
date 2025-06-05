using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaralESuvidha.Models;
using SaralESuvidha.ViewModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaralESuvidha.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashFlowController : ControllerBase
    {
        private IConfiguration _config;
        public CashFlowController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        [Authorize]
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
        [Authorize]
        [Route("GetRetailerUsers")]
        public List<UserInfo> GetRetailerUsers()
        {
            return StaticData.GetUsersByUserType(5);
        }

        [HttpGet]
        [Authorize]
        [Route("GetCollectorUsers")]
        public List<UserInfo> GetCollectorUsers()
        {
            return StaticData.GetUsersByUserType(12);
        }

        [HttpGet]
        [Authorize]
        [Route("GetCashierUsers")]
        public List<UserInfo> GetCashierUsers()
        {
            return StaticData.GetUsersByUserType(13);
        }

        [HttpGet]
        [Authorize]
        [Route("GetMappedUsersByCollectorId")]
        public List<MappedUserInfo> GetMappedUsersByCollectorId(string userId)
        {
            return StaticData.GetMappedUsersByCollectorId(userId);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public UserInfo Login(string userId, string password)
        {
            var user = StaticData.CashFlowLogin(userId, password);
            if(user != null && user.Message == "Success: Logedin successfully")
            {
                var tokenDetails = GenerateJSONWebToken(user);
                user.Token = tokenDetails.Item1;
                user.Expiry = tokenDetails.Item2;
            }
            return user;
        }

        private (string, DateTime) GenerateJSONWebToken(UserInfo userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Name, userInfo.UserName),
                new Claim(JwtRegisteredClaimNames.Sid, userInfo.Id),
                new Claim(JwtRegisteredClaimNames.Typ, userInfo.UserType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var exp = DateTime.Now.AddMinutes(120);
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: exp,
                signingCredentials: credentials);
            
            return (new JwtSecurityTokenHandler().WriteToken(token), exp);
        }


        [HttpGet]
        [Authorize]
        [Route("GetMasterData")]
        public MasterData GetMasterData()
        {
            return StaticData.GetMasterData();
        }

        [HttpPost]
        [Authorize]
        [Route("AlignCollectorWithRetailerUser")]
        public StringResult AlignCollectorWithRetailerUser(CollectorRetailerMapping mapping)
        {
            return new StringResult { Response = StaticData.AlignCollectorWithRetailerUser(mapping.CollectorId, mapping.RetailerId) };
        }

        [HttpGet]
        [Authorize]
        [Route("GetLiabilityAmountByRetailerId")]
        public LiabilityInfo GetLiabilityAmountByRetailerId(string userId)
        {
            return StaticData.GetLiabilityAmountByRetailerId(userId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLiabilityAmountByCollectorId")]
        public LiabilityInfo GetLiabilityAmountByCollectorId(string userId)
        {
            return StaticData.GetLiabilityAmountByCollectorId(userId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLiabilityAmountByCashierId")]
        public LiabilityInfo GetLiabilityAmountByCashierId(string userId)
        {
            return StaticData.GetLiabilityAmountByCashierId(userId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetMappedCollectorsByRetailerId")]
        public List<MappedUserInfo> GetMappedCollectorsByRetailerId(string userId)
        {
            return StaticData.GetMappedCollectorsByRetailerId(userId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLiabilityAmountOfAllRetailers")]
        public List<LiabilityInfo> GetLiabilityAmountOfAllRetailers()
        {
            return StaticData.GetLiabilityAmountOfAllRetailers();
        }

        [HttpPost]
        [Authorize]
        [Route("AddLadgerInfo")]
        public BoolResult AddLadgerInfo(LadgerInfo ladger)
        {
            ladger.GivenOn = DateTime.Now;
            return new BoolResult { Response = StaticData.AddLadgerInfo(ladger) };
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateLadgerInfo")]
        public BoolResult UpdateLadgerInfo(LadgerInfo ladger)
        {
            return new BoolResult { Response = StaticData.UpdateLadgerInfo(ladger) };
        }

        [HttpGet]
        [Authorize]
        [Route("GetLadgerInfoByRetailerid")]
        public List<Ladger> GetLadgerInfoByRetailerid(bool all, string retailerId)
        {
            return StaticData.GetLadgerInfoByRetailerid(all, retailerId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLadgerInfoByCollectorId")]
        public List<Ladger> GetLadgerInfoByCollectorId(bool all, string collectorId)
        {
            return StaticData.GetLadgerInfoByCollectorId(all, collectorId);
        }
        
        [HttpGet]
        [Authorize]
        [Route("GetLadgerInfoCreatedByCashierId")]
        public List<Ladger> GetLadgerInfoCreatedByCashierId(bool all, string cashierId)
        {
            return StaticData.GetLadgerInfoCreatedByCashierId(all, cashierId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLadgerInfoByRetaileridAndCollectorId")]
        public List<Ladger> GetLadgerInfoByRetaileridAndCollectorId(bool all, string retailerId, string collectorId)
        {
            return StaticData.GetLadgerInfoByRetaileridAndCollectorId(all, retailerId, collectorId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetLadgerInfosCreatedByCollectors")]
        public List<Ladger> GetLadgerInfosCreatedByCollectors(DateTime date)
        {
            return StaticData.GetLadgerInfosCreatedByCollectors(date);
        }

        [HttpDelete]
        [Authorize]
        [Route("DeleteLadgerInfo")]
        public StringResult DeleteLadgerInfo(int id)
        {
            return new StringResult { Response = StaticData.DeleteLadgerInfo(id) };
        }

        [HttpGet]
        [Authorize]
        [Route("GetCollectorLiabilities")]
        public List<LiabilityInfo> GetCollectorLiabilities()
        {
            return StaticData.GetCollectorLiabilities();
        }

        [HttpGet]
        [Authorize]
        [Route("GetCashierLiabilities")]
        public List<LiabilityInfo> GetCashierLiabilities()
        {
            return StaticData.GetCashierLiabilities();
        }

        [HttpGet]
        [Authorize]
        [Route("GetCollectorLiabilityDetails")]
        public List<Ladger> GetCollectorLiabilityDetails(string collectorId)
        {
            return StaticData.GetCollectorLiabilityDetails(collectorId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetCashierLiabilityDetails")]
        public List<Ladger> GetCashierLiabilityDetails(string cashierId)
        {
            return StaticData.GetCashierLiabilityDetails(cashierId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetCollectorLedgerDetails")]
        public List<Ladger> GetCollectorLedgerDetails(string collectorId)
        {
            return StaticData.GetCollectorLedgerDetails(collectorId);
        }
        
        [HttpGet]
        [Authorize]
        [Route("GetCashierLedgerDetails")]
        public List<Ladger> GetCashierLedgerDetails(string cashierId)
        {
            return StaticData.GetCashierLedgerDetails(cashierId);
        }

        [HttpGet]
        [Authorize]
        [Route("GetPendingApprovalLedgers")]
        public List<Ladger> GetPendingApprovalLedgers(bool showAll, int userType)
        {
            return StaticData.GetPendingApprovalLedgers(showAll, userType);
        }

        [HttpGet]
        [Authorize]
        [Route("GetUserExtendedInfo")]
        public List<UserEx> GetUserExtendedInfo()
        {
            return StaticData.GetUserExtendedInfo();
        }

        [HttpGet]
        [Authorize]
        [Route("GetLinkedCollectors")]
        public List<CollectorInfo> GetLinkedCollectors(string userId)
        {
            return StaticData.GetLinkedCollectors(userId);
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateIsSelfSubmitterFlag")]
        public BoolResult UpdateIsSelfSubmitterFlag(SubmitterFlagData data)
        {
            return new BoolResult { Response = StaticData.UpdateIsSelfSubmitterFlag(data) };
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateIsThirdPartyFlag")]
        public BoolResult UpdateIsThirdPartyFlag(ThirdpartyFlagData data)
        {
            return new BoolResult { Response = StaticData.UpdateIsThirdPartyFlag(data) };
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateOpeningBalanceData")]
        public BoolResult UpdateOpeningBalanceData(OpeningBalanceData data)
        {
            return new BoolResult { Response = StaticData.UpdateOpeningBalanceData(data) };
        }

        [HttpPost]
        [Authorize]
        [Route("LinkAllRetailersToNewCollector")]
        public BoolResult LinkAllRetailersToNewCollector(LinkingInfo data)
        {
            return new BoolResult { Response = StaticData.LinkAllRetailesToNewCollector(data) };
        }

        [HttpGet]
        [Authorize]
        [Route("GetLiabilityAmountOfAllRetailersByCollectorId")]
        public List<LiabilityInfo> GetLiabilityAmountOfAllRetailersByCollectorId(string collectorId)
        {
            return StaticData.GetLiabilityAmountOfAllRetailersByCollectorId(collectorId);
        }
        
        [HttpGet]
        [Authorize]
        [Route("GetPendingApprovalLedgersByCollectorId")]
        public List<Ladger> GetPendingApprovalLedgersByCollectorId(string collectorId, bool showAll)
        {
            return StaticData.GetPendingApprovalLedgersByCollectorId(collectorId, showAll);
        }

        [HttpGet]
        [Authorize]
        [Route("GetPassword")]
        public StringResult GetPassword(string userId)
        {
            return new StringResult { Response = StaticData.GetPassword(userId) };
        }

        [HttpPost]
        [Authorize]
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
        public decimal CollectorInitiatedAmount { get; set; }
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
        public string DocId { get; set; }
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
        public string DocId { get; set; }
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
