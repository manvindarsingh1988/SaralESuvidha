Alter table [dbo].[UPPCLConfig]
Add OTS_EligibilityCheck_Url varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_AmountDetails_Url varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_Init_Url varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_Consumer_key varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_Consumer_Secret varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_ExpiresIn int

Alter table [dbo].[UPPCLConfig]
Add OTS_ExpiresTime datetime

Alter table [dbo].[UPPCLConfig]
Add OTS_AccessToken varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_RefreshToken varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_TokenType varchar(500)

Alter table [dbo].[UPPCLConfig]
Add OTS_Init_PostData varchar(4000)

Alter table [dbo].[UPPCLConfig]
Add OTS_Submit_PostData varchar(4000)

Go

update [UPPCLConfig] set OTS_EligibilityCheck_Url = 'https://ewallet-test.uppclonline.com:8280/ots/1.0.0/checkEligibility?accountId=_accountNo_&discom=_discomName_'
update [UPPCLConfig] set OTS_AmountDetails_Url = 'https://ewallet-test.uppclonline.com:8280/ots/1.0.0/amountDetails?accountId=_accountNo_&discom=_discomName_'
update [UPPCLConfig] set OTS_Init_Url = 'https://ewallet-test.uppclonline.com:8280/ots/1.0.0/caseInit'
update [UPPCLConfig] set OTS_Consumer_key = '4xif1VMiXQ8lOSPOpYlFegJfofYa'
update [UPPCLConfig] set OTS_Consumer_Secret = 'VCju90u2oCnfXPv5W5xTy9qp6q4a'
update [UPPCLConfig] set OTS_Init_PostData = 
'{
"accountId": "_account_",
"discom": "_discom_",
"transitionMode": "CM_INITIATE",
"lpscAmount": "_lpscAmount_",
"supplyTypeCode": "_supplyTypeCode_",
"totalOutstandingAmt": "_totalOutstandingAmt_",
"principalAmt": "_principalAmt_",
"registrationFee": "_registrationFee_",
"downPayment": "_downPayment_",
"existingLoad": "_existingLoad_",
"installmentAmt": "_installmentAmt_",
"noOfInstallment": "_noOfInstallment_",
"registrationOption": "_registrationOption_",
"lpscWaiveOff": "_lpscWaiveOff_",
"extraParm1": "PAYMENT1",
"extraParm2": "",
"extraParm3": "CM_OTS2024"
}'

update [UPPCLConfig] set OTS_Submit_PostData = 
 '{
"agencyType": "OTHER",
"agentId": "_agency_id_",
"amount": _payable_amount_,
"billAmount" : _bill_amount_from_fetch_api_,
"billId": "_bill_id_from_fetch_api_",
"connectionType": "_connection_type_from_fetch_api_",
"consumerAccountId": "_consumer_account_number_",
"consumerName": "_consumer_name_",
"discom": "_discom_from_fetch_api_",
"division": "_division_from_fetch_api_",
"divisionCode": "_division_code_from_fetch_api_",
"mobile": "_payer_mobile_number_",
"outstandingAmount" : _account_info_from_fetch_api_,
"paymentType" : "_payment_type_Full_or_PARTIAL_",
"referenceTransactionId": "_our_system_id_",
"sourceType": "_project_area_from_bill_fetch_api_",
"type": "_project_area_from_bill_fetch_api_",
"vanNo": "_van_no_",
"walletId": "_our_agent_id_",
"scheme": "OTS",
"param1": "_param1_",
"city": "_city_"
}'
Go

Alter PROC [dbo].[usp_UPPCLConfigSelect]   
    @Id varchar(10)  
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
  
 BEGIN TRAN  
  
 SELECT [Id], [AgentVANNo], [AgentID], [Token_APIUsername], [Token_APIPassword], [TokenUrl], [RefreshTokenUrl], [BillFetch_Discom_TokenConsumerKey], [BillFetch_Discom_TokenConsumerSecret], [BillFetch_Discom_AccessToken], [BillFetch_Discom_RefreshToken],
 [BillFetch_Discom_TokenType], [BillFetch_Discom_ExpiresIn], [BillFetch_Discom_ExpiresTime], [BillFetch_DiscomName_Url], [BillFetch_BillDetail_TokenConsumerKey], [BillFetch_BillDetail_TokenConsumerSecret], [BillFetch_BillDetail_AccessToken], 
 [BillFetch_BillDetail_RefreshToken], [BillFetch_BillDetail_TokenType], [BillFetch_BillDetail_ExpiresIn], [BillFetch_BillDetail_ExpiresTime], [BillFetch_BillDetail_Url], [BillFetch_BillDetail_PostData], [BillPost_Wallet_TokenConsumerKey], 
 [BillPost_Wallet_TokenConsumerSecret], [BillPost_Wallet_AccessToken], [BillPost_Wallet_RefreshToken], [BillPost_Wallet_TokenType], [BillPost_Wallet_ExpiresIn], [BillPost_Wallet_ExpiresTime], [BillPost_Wallet_AgentWalletUrl], [BillPost_BillPayment_TokenConsumerKey], 
 [BillPost_BillPayment_TokenConsumerSecret], [BillPost_BillPayment_AccessToken], [BillPost_BillPayment_RefreshToken], [BillPost_BillPayment_TokenType], [BillPost_BillPayment_ExpiresIn], [BillPost_BillPayment_ExpiresTime], [BillPost_BillPayment_BillPostUrl], 
 [BillPost_BillPayment_PostData], [BillPost_StatusCheck_TokenConsumerKey], [BillPost_StatusCheck_TokenConsumerSecret], [BillPost_StatusCheck_AccessToken], [BillPost_StatusCheck_RefreshToken], [BillPost_StatusCheck_TokenType], [BillPost_StatusCheck_ExpiresIn], 
 [BillPost_StatusCheck_ExpiresTime], [BillPost_StatusCheck_ApiUrl], [BillPost_StatusCheck_PostData], [Forcefail_TokenConsumerKey], [Forcefail_TokenConsumerSecret], [Forcefail_AccessToken], [Forcefail_RefreshToken], [Forcefail_TokenType], [Forcefail_ExpiresIn],
 [Forcefail_ExpiresTime], [Forcefail_FailUrl], [Forcefail_PostData], OTS_EligibilityCheck_Url, OTS_AmountDetails_Url, OTS_Init_Url, OTS_TokenType, OTS_RefreshToken, OTS_AccessToken, OTS_ExpiresTime, OTS_ExpiresIn, OTS_Consumer_Secret, OTS_Consumer_key, OTS_Init_PostData, OTS_Submit_PostData
 FROM   [dbo].[UPPCLConfig] WITH(NOLOCK)   
 WHERE  ([Id] = @Id OR @Id IS NULL)   
  
 COMMIT  


 go

 
Alter PROC [dbo].[usp_TokenExpiry]  
AS   
  
SELECT DATEDIFF(MINUTE,GETDATE(),[BillFetch_Discom_ExpiresTime]) AS Discom,   
DATEDIFF(MINUTE,GETDATE(),[BillFetch_BillDetail_ExpiresTime]) AS BillDetail,   
DATEDIFF(MINUTE,GETDATE(),[BillPost_Wallet_ExpiresTime])  AS BillPostWallet,  
DATEDIFF(MINUTE,GETDATE(),[BillPost_BillPayment_ExpiresTime])  AS BillPostPayment,  
DATEDIFF(MINUTE,GETDATE(),[BillPost_StatusCheck_ExpiresTime])  AS BillPostStatusCheck, --  
DATEDIFF(MINUTE,GETDATE(),[Forcefail_ExpiresTime])  AS Forcefail,  
case when OTS_ExpiresTime is null then -1 else DATEDIFF(MINUTE,GETDATE(),OTS_ExpiresTime) end AS OTS,
'Success' AS [Message]  
FROM UPPCLConfig WHERE Id='CON001'  

  
  Go

CREATE PROC [dbo].[usp_UPPCLConfigOTSTokenUpdate]   
    @Id varchar(10),  
    @OTS_AccessToken varchar(4000) = NULL,  
    @OTS_RefreshToken varchar(4000) = NULL,  
    @OTS_TokenType varchar(500) = NULL,  
    @OTS_ExpiresIn int = NULL,  
    @OTS_ExpiresTime datetime = NULL  
      
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
   
 BEGIN TRAN  
  
 UPDATE [dbo].[UPPCLConfig]  
 SET    
 [OTS_AccessToken] = @OTS_AccessToken,   
 [OTS_RefreshToken] = @OTS_RefreshToken,   
 [OTS_TokenType] = @OTS_TokenType,   
 [OTS_ExpiresIn] = @OTS_ExpiresIn,   
 [OTS_ExpiresTime] = @OTS_ExpiresTime  
 WHERE  [Id] = @Id  
   
 SELECT 'Successfully updated token.' AS 'OperationMessage';  
  
 COMMIT  