Alter Table RTran
Add UPPCL_LifelineAct nvarchar(10)

Go

Alter PROC [dbo].[usp_RTranValidateUPPCL]       
 @RetailUserOrderNo int,      
    @MobileNumber varchar(50) = NULL,      
    @OperatorName varchar(100) = NULL,      
 @Amount decimal(10,2),      
 @RechargeType varchar(3),          
    @RequestIP varchar(300) = NULL,         
 @RequestMachine varchar(800) = NULL,       
 @RequestGeoCode varchar(30) = NULL,       
 @RequestNumber varchar(20) = NULL,       
 @RequestMessage varchar(200) = NULL,       
 @RequestTime DATETIME = NULL,      
 @Parameter1 nvarchar(50) = NULL,      
 @Parameter2 nvarchar(50) = NULL,      
 @Parameter3 nvarchar(50) = NULL,      
 @Parameter4 nvarchar(50) = NULL,      
 @EndCustomerName nvarchar(100) = NULL,      
 @EndCustomerMobileNumber varchar(20) = NULL,      
 @Extra1 nvarchar(50) = NULL,      
 @Extra2 nvarchar(50) = NULL,      
 @UPPCL_ProjectArea varchar(30) = NULL,      
 @UPPCL_AccountInfo decimal(18, 2) = NULL,      
    @UPPCL_TDConsumer bit = NULL,      
    @UPPCL_ConnectionType varchar(30) = NULL,      
    @UPPCL_DivCode varchar(30) = NULL,      
    @UPPCL_SDOCode varchar(30) = NULL,      
    @UPPCL_BillAmount decimal(18, 2) = NULL,      
    @UPPCL_Division varchar(30) = NULL,      
    @UPPCL_SubDivision varchar(30) = NULL,      
    @UPPCL_PurposeOfSupply varchar(30) = NULL,      
    @UPPCL_SanctionedLoadInKW decimal(18, 6) = NULL,      
    @UPPCL_BillId varchar(50) = NULL,      
 @UPPCL_Discom varchar(150) = NULL,      
 @UPPCL_BillDate varchar(30) = NULL,      
 @UPPCL_PaymentType varchar(20) = NULL,      
 @UPPCL_DDR INT = 0,      
 @ClientApiUserReferenceId nvarchar(50) = NULL,    
 @IsOTS tinyint = 0 ,  
 @IsFull tinyint = 0 ,
 @UPPCL_LifelineAct nvarchar(10)
 --@OutResult varchar(500) = NULL      
AS       
 SET NOCOUNT ON       
 SET XACT_ABORT ON        
 DECLARE @RetailUserId varchar(10);      
 DECLARE @OperationMessage varchar(500);       
 DECLARE @IsValidOperator bit = 0;      
 --DECLARE @OperatorNameExpression varchar(150);      
 --DECLARE @AmountExpression varchar(800);      
 DECLARE @CurrentBalance decimal(18,2) = 0;      
 DECLARE @DailyLimit decimal(10,2) = 0;      
 DECLARE @UserOperatorName varchar(100) = @OperatorName;      
 DECLARE @RepeatValidationPass bit = 0;      
 DECLARE @DailyLimitPass bit = 0;      
 DECLARE @RepeatDelay INT = 0;      
 DECLARE @RepeatDelayInDb INT = 0;      
 DECLARE @IsRepeatAllowedInDb INT = 0;      
 DECLARE @OperatorCircle varchar(50);      
 DECLARE @CommissionMargin decimal(10,2); -- =0      
 DECLARE @Margin decimal(18,2) = 0;      
 DECLARE @MaxMargin decimal(10,2) = 0;      
 DECLARE @CreditAmount DECIMAL(18,2) = 0;      
 DECLARE @DebitAmount DECIMAL(18,2) = 0;      
 DECLARE @ClosingBalance DECIMAL(18,2) = 0;      
 DECLARE @SalePrice decimal(18,2) = 0;      
 DECLARE @CGst decimal(18,2) = 0;      
 DECLARE @SGst decimal(18,2) = 0;      
 DECLARE @IGst decimal(18,2) = 0;      
 DECLARE @RTranId varchar(14);      
 DECLARE @OperatorType varchar(30) = '';      
 DECLARE @RechargeStatus varchar(10) = 'PROCESS';      
 DECLARE @UtilityCommissionFound SMALLINT = 0;      
       
       
 BEGIN TRAN      
      
  SELECT @MobileNumber = LTRIM(RTRIM(@MobileNumber));      
       
  SELECT @RetailUserId = Id, @RepeatDelayInDb = RepeatDelayInSecond, @IsRepeatAllowedInDb = RepeatPaymentAllowed from RetailUser WITH(NOLOCK) WHERE OrderNo=@RetailUserOrderNo;      
        
   --Proceed with furher details.      
   SELECT @OperatorCircle=ISNULL(OperatorCircle,'UNKNOWN') from OperatorIndex WITH(NOLOCK) WHERE NumberPrefix=CONVERT(INT,SUBSTRING(@MobileNumber,0,5));      
      
   SELECT @IsValidOperator = Active, @OperatorType=OperatorType from OperatorMaster WITH(NOLOCK) WHERE OperatorName=@OperatorName; --, @OperatorNameExpression=ValidationExpression, @AmountExpression=ValidDenomination      
         
   IF(@IsValidOperator=1 AND @Amount>0)      
   BEGIN      
    DECLARE @RetailUserOperatorActive tinyint;      
    --SELECT @RetailUserOperatorActive = ISNULL(OrderNo,0), @CommissionMargin = ISNULL(CommissionMargin,0) from OperatorMargin WITH(NOLOCK) WHERE @RetailUserId=@RetailUserId AND OperatorName=@OperatorName AND Active=1;      
    SELECT @RetailUserOperatorActive = 1;      
      
    IF(@RetailUserOperatorActive>0)      
    BEGIN      
     SELECT @RepeatDelay = ISNULL(DATEDIFF(SECOND,(SELECT TOP 1 CreateDate from [RTRan] WITH(nolock) WHERE TelecomOperatorName=@OperatorName AND RechargeMobileNumber=@MobileNumber AND Amount=@Amount AND RechargeType=@RechargeType AND TranType=3 AND RechargeStatus='PROCESS' AND Convert(DATE,CreateDate)=CONVERT(DATE, GETDATE()) ORDER BY CreateDate DESC),GETUTCDATE()+'05:30'),0); --           
         
     IF(@RepeatDelay>0)      
     BEGIN      
      SELECT @RepeatValidationPass = 0;      
      SELECT @OperationMessage = 'Errors: Repeat is not allowed and a same recharge is already processed for today.';      
      SET @RechargeStatus = 'INVALID';      
     END      
     IF(@RepeatDelay=0) --(@RepeatAllowed='0' OR @RepeatAllowed='1') AND       
     BEGIN      
      SELECT @RepeatValidationPass = 1;      
     END      
      
     IF(@RepeatValidationPass=1)      
     BEGIN      
      DECLARE @DailyLimitInConfig decimal = 99999999;      
      --SELECT @DailyLimitInConfig = ISNULL(MaxLimit,0) from OperatorMargin WITH(NOLOCK) WHERE ApiUserClientId=@ApiUserId AND OperatorName=@OperatorName AND Active=1;      
      --SELECT @DailyLimit = ISNULL(SUM(DebitAmount),0) from RTran WITH(NOLOCK) WHERE TranType=3 AND TelecomOperatorName=@OperatorName AND RechargeStatus<>'FAILED' AND Convert(DATE,CreateDate)=CONVERT(DATE, GETDATE());      
      IF(@DailyLimitInConfig>@DailyLimit) --@DailyLimitInConfig=0 OR       
      BEGIN      
       SELECT @DailyLimitPass = 1;      
      END      
      ELSE      
      BEGIN      
       SELECT @OperationMessage = 'Errors: Operator daily limit for today is exceeded.';       
       SET @RechargeStatus = 'INVALID';      
      END      
     END      
      
     IF(@DailyLimitPass = 1)      
     BEGIN      
      --EXEC @CurrentBalance = [dbo].[ApiUserClientBalance] @ApiUserId;      
      SELECT @CurrentBalance = [dbo].[RetailUserBalance](@RetailUserId);      
      SELECT @ClosingBalance = @CurrentBalance;      
      IF(@CurrentBalance>0 AND @CurrentBalance>=@Amount)      
      BEGIN      
       SELECT @Margin = (@Amount * @CommissionMargin)/100;      
                            IF(@OperatorType='Mobile' OR @OperatorType='DTH')      
                            BEGIN      
                                SELECT @CommissionMargin=ISNULL(MarginPercent,0), @MaxMargin=ISNULL(MaxMargin,0) FROM UtilityMargin WITH(NOLOCK) WHERE UtilityType=@OperatorType AND Active=1 AND RetailUserid=@RetailUserId AND OperatorName=@OperatorName;   
  
   
                            END      
                            ELSE      
                            BEGIN      
           SELECT @CommissionMargin=ISNULL(MarginPercent,0), @MaxMargin=ISNULL(MaxMargin,0) FROM UtilityMargin WITH(NOLOCK) WHERE UtilityType=@OperatorType AND Active=1 AND RetailUserid=@RetailUserId;      
                            END      
      
       IF(@CommissionMargin IS NOT NULL)      
       BEGIN      
        SELECT @Margin = (@Amount * @CommissionMargin)/100;      
        IF(@MaxMargin>0 AND @Margin>@MaxMargin)      
        BEGIN      
         SELECT @Margin = @MaxMargin;      
        END      
       END      
       ELSE-- IF(@CommissionMargin=0 AND @UtilityCommissionFound=99)      
       BEGIN      
        SELECT @CommissionMargin=ISNULL(MarginPercent,0), @MaxMargin=ISNULL(MaxMargin,0) FROM UtilityMargin WHERE UtilityType=@OperatorType AND Active=1 AND RetailUserid IS NULL;      
        IF(@CommissionMargin>0)      
        BEGIN      
         SELECT @Margin = (@Amount * @CommissionMargin)/100;      
         IF(@MaxMargin>0 AND @Margin>@MaxMargin)      
         BEGIN      
          SELECT @Margin = @MaxMargin;      
         END      
        END      
       END      
      
                            IF(@CommissionMargin IS NULL OR @Parameter2 = 'PartPayment')      
                            BEGIN      
            SELECT @Margin = 0;      
                            END      
       --set all margin to zero      
       SELECT @Margin = 0;      
      
       SELECT @DebitAmount = @Amount - @Margin;      
       SELECT @ClosingBalance = @CurrentBalance - @DebitAmount; --+ @CreditAmount      
       SELECT @RechargeType = 'R';      
       SELECT @OperatorCircle = 'UP';      
       SELECT @SalePrice = (@DebitAmount * 100)/118;      
       SELECT @IGst = @DebitAmount - @SalePrice;      
       SET @RechargeStatus = 'PROCESS';      
      
      
       SELECT @RTranId = RIGHT('R000' + convert(varchar(10), next value for RTranNextId),14);      
      
       INSERT INTO [dbo].[RTran] ([Id], [RetailUserId], [OurApiId], [TelecomOperatorName], [OurApiOperator], [ApiOperatorCode], [OperatorCircle], [RechargeMobileNumber], [Amount], [FinalAmount],[Parameter1], [Parameter2], [Parameter3], [Parameter4], [OpeningBalance],       
       [DebitAmount], [CreditAmount], [ClosingBalance], [SalePrice], [CGst], [SGst], [IGst], [Margin], [RequestIp], [RequestMachine], [RequestGeoCode], [RequestNumber], [RequestMessage],      
       [RequestTime], [TranType], [RechargeType], [RechargeStatus], [Gateway], [Remarks], [RetailUserOrderNo], [EndCustomerName], [Extra1], [Extra2], [CreateDate],[UPPCL_ProjectArea], [UPPCL_TDConsumer], [UPPCL_ConnectionType], [UPPCL_DivCode], [UPPCL_SDOCode],       
       [UPPCL_AccountInfo], [UPPCL_BillAmount], [UPPCL_Division], [UPPCL_SubDivision], [UPPCL_PurposeOfSupply], [UPPCL_SanctionedLoadInKW], [UPPCL_BillId], [UPPCL_Discom], [EndCustomerMobileNumber], [UPPCL_BillDate], [UPPCL_PaymentType], [UPPCL_DDR], [ClientApiUserReferenceId], [IsOTS], [IsFull], UPPCL_LifelineAct)      
       SELECT @RTranId, @RetailUserId, 'A00001',  @OperatorName, @OperatorName, @OperatorName, @OperatorCircle, @MobileNumber, @Amount, @Amount, @Parameter1, @Parameter2, @Parameter3, @Parameter4, @CurrentBalance,       
       @DebitAmount, @CreditAmount, @ClosingBalance, @SalePrice, @CGst, @SGst, @IGst, @Margin, @RequestIp, @RequestMachine, @RequestGeoCode, @RequestNumber, @RequestMessage,       
       @RequestTime, 3, @RechargeType, @RechargeStatus, 0, @OperationMessage, @RetailUserOrderNo, @EndCustomerName, @Extra1, @Extra2, GETDATE(),      
       @UPPCL_ProjectArea, @UPPCL_TDConsumer, @UPPCL_ConnectionType, @UPPCL_DivCode, @UPPCL_SDOCode, @UPPCL_AccountInfo, @UPPCL_BillAmount, @UPPCL_Division, @UPPCL_SubDivision, @UPPCL_PurposeOfSupply, @UPPCL_SanctionedLoadInKW, @UPPCL_BillId, @UPPCL_Discom, @EndCustomerMobileNumber, @UPPCL_BillDate, @UPPCL_PaymentType,@UPPCL_DDR,@ClientApiUserReferenceId, @IsOTS, @IsFull, @UPPCL_LifelineAct;      
                      
       SELECT @OperationMessage = 'Success: Recharge request successfully received.';      
      END      
      ELSE      
      BEGIN      
       SELECT @OperationMessage = 'Errors: Insufficient fund in account, can not accept request.';      
       SET @RechargeStatus = 'INVALID';      
      END      
     END      
      
    END      
    ELSE      
    BEGIN      
     SELECT @OperationMessage = 'Errors: Operator not active for this user.';       
     SET @RechargeStatus = 'INVALID';      
    END      
   END      
   ELSE      
   BEGIN      
    SELECT @OperationMessage = 'Errors: Invalid operator, not found or disabled.';       
    SET @RechargeStatus = 'INVALID';      
   END      
        
        
       
       
 SELECT @RTranId as Id, @OperatorName as OperatorName, @MobileNumber as MobileNumber, @Amount as Amount,@RechargeType as RechargeType,@RechargeStatus as RechargeStatus, @CurrentBalance as OpeningBalance, @DebitAmount as DebitAmount, @ClosingBalance as ClosingBalance, @OperationMessage as OperationMessage;      
      
 COMMIT 

 Go 

Update [dbo].[UPPCLConfig]
set BillPost_BillPayment_PostData = 
 '{  "agencyType": "OTHER",  "agentId": "_agency_id_",  "amount": _payable_amount_,  "billAmount" : _bill_amount_from_fetch_api_,  "billId": "_bill_id_from_fetch_api_",  "connectionType": "_connection_type_from_fetch_api_",  "consumerAccountId": "_consumer_account_number_",  "consumerName": "_consumer_name_",  "discom": "_discom_from_fetch_api_",  "division": "_division_from_fetch_api_",  "divisionCode": "_division_code_from_fetch_api_",  "mobile": "_payer_mobile_number_",  "outstandingAmount" : _account_info_from_fetch_api_,  "paymentType" : "_payment_type_Full_or_PARTIAL_",  "referenceTransactionId": "_our_system_id_",  "sourceType": "_project_area_from_bill_fetch_api_",  "type": "_project_area_from_bill_fetch_api_",  "vanNo": "_van_no_",  "walletId": "_our_agent_id_",  "city": "_city_",  "param1": "_param1_", "tdStatus": "_TDStatus_", "lifeLineAct": "_LifelineAct_" }'
 
 
 Go
 
 Alter procedure [dbo].[usp_RetailUserListWithUPPCLCommission]  
@dateFrom datetime,  
@dateTo datetime,  
@Id varchar(10)  
As  
Begin  
  
  Select RetailUserId,  
  Sum(Case when RTRim(LTRIM(UPPCL_PaymentType)) = 'FULL' and Amount < cast(2000 as decimal) then 20 else amount * .5 / 100 end) UPPCL_Commission, 
  Sum(Amount) Amount
  into #temp   
  from RTran r   
  Left join RetailUser t with (nolock) on t.Id = r.RetailUserId    
  Where cast(CreateDate as date) between cast(@dateFrom as date) and cast(@dateTo as date) and RTRim(LTRIM(UPPCL_PaymentType)) in ('FULL', 'PARTIAL') and RechargeStatus = 'Success'  
  and 1 = case when ((@Id is not null and t.OrderNo = @Id) or @Id is null) then 1 else 0 end  
  Group By RetailUserId  
  
  declare @Days int  
  set @Days = DATEDIFF(DAY,@dateFrom,@dateTo) + 1  
    
  
 SELECT RU.Id, OrderNo AS USL, Case When UserType = 5 then 'Retailer' when UserType = 6 then 'Distributor' else 'Master Distributor' end UserType,   
 [dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName,  
 ISNULL(FirstName,'')+' '+ISNULL(MiddleName,'')+' ' + ISNULL(LastName,'') As RName,  
 Mobile AS MobileNumber,        
 MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName,  
 City, [Address], Active,  
 UPPCL_Commission, ( (IsNULL(Wages,0)/30) * @Days) as Salary,
 Amount, EMail,
(UPPCL_Commission / Amount * 100) CommissionPercentage 
 FROM RetailUser RU WITH(NOLOCK) Left join #temp t on t.RetailUserId = RU.Id    
 where UserType = 5 and 1 = case when ((@Id is not null and OrderNo = @Id) or @Id is null) then 1 else 0 end   
 ORDER BY OrderNo ASC   
End

