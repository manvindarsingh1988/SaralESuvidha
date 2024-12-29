CREATE PROC [dbo].[usp_PaymentOTSReceiptById]   
@Id varchar(20)  
AS   
SELECT  RechargeMobileNumber AccountId, RechargeStatus, Amount, IsFull
FROM RTran WITH(NOLOCK) WHERE Id=@Id;  

Go

Alter proc usp_GetApiResponseByApiTypeAndConsumerId  
@ConsumerNumber varchar(50),  
@ApiType varchar(30), 
@Transid varchar(20)
As   
begin 
	if(@Transid is not null)
	begin
		declare @CreateDate datetime
		Select @CreateDate = CreateDate from UPPCLHitLog where RTranId = @Transid
		Select Top 1 ResponseData from UPPCLHitLog Where ConsumerNumber = @ConsumerNumber and ApiType = @ApiType and CreateDate < @CreateDate order by CreateDate Desc 
	end
	else
	begin
		Select Top 1 ResponseData from UPPCLHitLog Where ConsumerNumber = @ConsumerNumber and ApiType = @ApiType order by CreateDate Desc 
	end
end

  
Go
  
  
Alter PROC [dbo].[usp_RechargeReportRetailClientByDate]   
    @TranDateFrom DATE, @TranDateTo DATE, @OrderNo INT  
AS   
 DECLARE @RetailUserId varchar(14)  
 SELECT @RetailUserId = Id from RetailUser WITH(NOLOCK) WHERE OrderNo=@OrderNo;  
 Select Id as Rid, RefundTransactionId as RefundId, TelecomOperatorName as OperatorName, OperatorCircle,Amount,   
 DebitAmount as Debit, CreditAmount as Credit,Margin, OpeningBalance as OB, ClosingBalance as CB,RechargeMobileNumber as RechargeNumber, CASE RechargeStatus WHEN 'PROCESS' THEN 'PROCESS' ELSE RechargeStatus END AS RechargeStatus, ISNULL(Remarks,'') as Remarks, CreateDate, LiveId, ISOTS  from [RTran] WITH(NOLOCK) --  
 WHERE (CONVERT(DATE,CreateDate) BETWEEN @TranDateFrom AND @TranDateTo) AND RetailUserId=@RetailUserId ORDER BY CreateDate 
 
 Go

 Alter PROC [dbo].[usp_RechargeReportDistributorByDate]   
    @TranDateFrom DATE, @TranDateTo DATE, @OrderNo INT  
AS   
 DECLARE @RetailUserId varchar(14)  
 SELECT @RetailUserId = Id from RetailUser WITH(NOLOCK) WHERE OrderNo=@OrderNo;  
 Select Id as Rid, RetailUserId AS Retailer, RefundTransactionId as RefundId, TelecomOperatorName as OperatorName, OperatorCircle,Amount,   
 DebitAmount as Debit, CreditAmount as Credit,Margin, OpeningBalance as OB, ClosingBalance as CB,RechargeMobileNumber as RechargeNumber, CASE RechargeStatus WHEN 'PROCESS' THEN 'PROCESS' ELSE RechargeStatus END AS RechargeStatus, ISNULL(Remarks,'') as Remarks, CreateDate, LiveId, UPPCL_PaymentType AS PaymentType, ISOTS  from [RTran] WITH(NOLOCK) --  
 WHERE (CONVERT(DATE,CreateDate) BETWEEN @TranDateFrom AND @TranDateTo) AND   
 RetailUserId IN(SELECT Id FROM RetailUser WHERE MasterId=@RetailUserId) ORDER BY CreateDate  
 
 Go
 Alter Table [SystemSetting]
 Add IsOTSDown bit

 Go

 Update [SystemSetting] set IsOTSDown = 0;
 
 Go

Alter PROC [dbo].[usp_SystemSettingUpdate]   
    @IsDown bit = NULL,  
    @IsDownMessage nvarchar(500) = NULL,
	@IsOTSDown bit = null
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
   
 BEGIN TRAN  
  
 UPDATE [dbo].[SystemSetting]  
 SET   [IsDown] = @IsDown, [IsDownMessage] = @IsDownMessage, IsOTSDown = @IsOTSDown;  
  
 SELECT 'Success: Successfully updated.' AS 'OperationMessage';  
  
 COMMIT  