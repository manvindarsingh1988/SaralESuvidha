  Alter Table UtilityMargin
  Add MarginPercentUpto200 float
Go
  
 
Alter PROC [dbo].[usp_OperatorMarginUpdateRetail]   
@OrderNo BIGINT, @OperatorName varchar(50), @MarginPercent decimal(10,2), @Maxmargin decimal(10,2), @FixMargin decimal(10,2), @MaxDailyLimit decimal(14,2), @Remarks nvarchar(500), @CreatedByUser varchar(20), @LastUpdateDate DATETIME, @LastUpdateMachine VARCHAr(500), @Active BIT, @MarginPercentUpto200 float  
AS   
 DECLARE @Id BIGINT DECLARE @RetailUserid varchar(10) DECLARE @UtilityType varchar(50) DECLARE @OperationMessage varchar(500) = 'START'  DECLARE @ExistingMarginPercent decimal(10,2)=0 DECLARE @ExistingMaxMargin decimal(10,2)=0 DECLARE @ExistingFixMargin decimal(10,2)=0, @ExistingMarginPercentUpto200 float = 0  
 DECLARE @RetailerName varchar(300) DECLARE @MasterId varchar(10) = NULL DECLARE @ValidData BIT = 1 DECLARE @AppUserFound INT = 0  DECLARE @ExistingMarginPercentParent decimal(10,2)=0 DECLARE @ExistingMaxMarginParent decimal(10,2)=0 DECLARE @ExistingFixMarginParent decimal(10,2)=0, @ExistingMarginPercentUpto200Parent float = 0 --DECLARE @CreatedByType VARCHAr(50) = 'RETAIL'  
   
 SELECT @RetailUserid = Id, @MasterId = ISNULL(MasterId,''), @RetailerName = FirstName + ' ' + ISNULL(MiddleName,'') + ' ' + ISNULL(LastName,'') FROM RetailUser WITH(NOLOCK) WHERE OrderNo=@OrderNo;  
 SELECT @Id = ISNULL(COUNT(Id),0) FROM UtilityMargin WITH(NOLOCK) WHERE OperatorName=@OperatorName AND RetailUserid=@RetailUserid;  
    SELECT @ExistingMarginPercent=ISNULL(MarginPercent,0), @ExistingMaxMargin=ISNULL(MaxMargin,0), @ExistingFixMargin=FixMargin, @ExistingMarginPercentUpto200 = MarginPercentUpto200  FROM UtilityMargin WITH(NOLOCK) WHERE OperatorName=@OperatorName AND RetailUserid=@RetailUserid;  
 SELECT @UtilityType = @OperatorName;  
   
 --IF(EXISTS(SELECT Id FROM AppUser WHERE Id=@CreatedByUser))  
 --BEGIN  
 -- SELECT @CreatedByType = 'INTERNAL';  
 --END  
  
 IF(@RetailUserid IS NULL)  
 BEGIN  
  SELECT @OperationMessage = 'Error: Invalid retailer data, can not set margin.';  
 END  
 ELSE  
 BEGIN  
  SELECT @OperationMessage = 'Valid retailer data.';  
  --IF(@CreatedByType != 'RETAIL')  
  SELECT @AppUserFound = ISNULL(COUNT(Id),0) FROM AppUser WHERE Id=@CreatedByUser;  
  IF(@AppUserFound=0 AND @MasterId != @CreatedByUser)  
  BEGIN  
   --IF(@SystemParentId != @CreatedByUser)  
   --BEGIN  
   -- SELECT @OperationMessage = 'Error: Invalid parent, can not set margin. You can only set margin for your direct downline.';  
   -- SELECT @ValidData = 0;  
   --END  
   SELECT @RetailerName = 'INVALID PARENT';  
   SELECT @OperationMessage = 'Error: Invalid parent, can not set margin. You can only set margin for your direct downline.';  
   SELECT @ValidData = 0;  
  END  
  
  IF(@ValidData=1)  
  BEGIN  
   SELECT @OperationMessage = 'Valid Data';  
  
            IF(@MasterId<>'')  
            BEGIN  
                --Get Parent user margin percent if user is not master distributor.  
                SELECT @ExistingMarginPercentParent=MarginPercent, @ExistingMaxMarginParent=MaxMargin, @ExistingFixMarginParent=FixMargin, @ExistingMarginPercentUpto200Parent = MarginPercentUpto200 FROM UtilityMargin WITH(NOLOCK) WHERE OperatorName=@OperatorName AND RetailUserid=@MasterId;  
            END  
  
            IF(@MasterId<>'' AND (@MarginPercent>@ExistingMarginPercentParent OR @MaxMargin>@ExistingMaxMarginParent OR @FixMargin>@ExistingFixMarginParent OR @MarginPercentUpto200>@ExistingMarginPercentUpto200Parent))  
            BEGIN  
                SELECT @OperationMessage = 'Error: Current margin is higher than parent(your) margin. Parent margin is - Margin % UPTO 200-' + CONVERT(VARCHAR(10),@ExistingMarginPercentUpto200Parent) + ', Margin-' + CONVERT(VARCHAR(10),@ExistingMarginPercentParent) + ', Max Margin-' + CONVERT(VARCHAR(10),@ExistingMaxMarginParent) + ', Fix Margin-' + CONVERT(VARCHAR(10),@ExistingFixMarginParent) + '. Lower the margin.';  
            END  
            ELSE  
            BEGIN  
                IF(EXISTS(SELECT OperatorType FROM OperatorMaster WITH(NOLOCK) WHERE OperatorName=@OperatorName))  
                BEGIN  
                    SELECT @UtilityType = OperatorType FROM OperatorMaster WITH(NOLOCK) WHERE OperatorName=@OperatorName;   
                END  
  
                IF (@Id=0)  
                BEGIN  
                    INSERT INTO [dbo].[UtilityMargin] ([CreatedByUser], [RetailUserid], [UtilityType], [OperatorName], [MarginPercent], [MaxMargin], [MaxDailyLimit], [FixMargin], [Active], [Remarks], [LastUpdateDate], [LastUpdateMachine], MarginPercentUpto200) SELECT @CreatedByUser, @RetailUserid, @UtilityType, @OperatorName, @MarginPercent, @MaxMargin, @MaxDailyLimit, @FixMargin, @Active, @Remarks, @LastUpdateDate, @LastUpdateMachine, @MarginPercentUpto200  
                    SELECT @OperationMessage = 'Margin successfully saved as ' + CONVERT(VARCHAR(10),@MarginPercent) + '% for operator ' + @OperatorName + ' for user - ' + @RetailerName + ' (' + CONVERT(VARCHAR(10),@OrderNo) + ').';  
                END  
                ELSE  
                BEGIN  
                    IF(@MarginPercent<@ExistingMarginPercent OR @MaxMargin<@ExistingMaxMargin)  
                    BEGIN  
                        --Lower the margins of all downline as margin or max margin is lowered.  
                        PRINT('Margin lowered');  
                        BEGIN TRAN  
                        --EXEC [dbo].[usp_OperatorMarginCommissionRevision] @OrderNo, @OperatorName, @MarginPercent, @Maxmargin, @Remarks  
        
                        COMMIT  
                    END  
  
                    BEGIN TRAN  
                    UPDATE [dbo].[UtilityMargin]  
                    SET [CreatedByUser] = @CreatedByUser, [RetailUserid] = @RetailUserid, [UtilityType] = @UtilityType, [OperatorName] = @OperatorName, [MarginPercent] = @MarginPercent, [MaxMargin] = @MaxMargin, [MaxDailyLimit] = @MaxDailyLimit, [FixMargin] = @FixMargin,   
                    [Active] = @Active, [Remarks] = @Remarks, [LastUpdateDate] = @LastUpdateDate, [LastUpdateMachine] = @LastUpdateMachine, MarginPercentUpto200 = @MarginPercentUpto200  
                    WHERE OperatorName=@OperatorName AND RetailUserid=@RetailUserid;  
                    COMMIT  
                      
                    --+ ' /Exis-' + CONVERT(VARCHAR(10),@ExistingMarginPercent)  
                    SELECT @OperationMessage = 'Margin successfully updated as ' + CONVERT(VARCHAR(10),@MarginPercent)  + '% for operator ' + @OperatorName + ' for user - ' + @RetailerName + ' (' + CONVERT(VARCHAR(10),@OrderNo) + ').';  
                END  
            END  
     
  END  
 END  
  
 -- + CONVERT(VARCHAR(10),@AppUserFound) + '-' + @MasterId + ' ' + @CreatedByUser  
 SELECT @RetailerName AS FullName, @OperatorName AS OperatorName, @FixMargin AS FixMargin, @MarginPercent AS MarginPercent, @Maxmargin AS MaxMargin, @MarginPercentUpto200 AS MarginPercentUpto200, @OperationMessage AS OperationMessage; --+ ' U-' + @UtilityType + ' ID-' + CONVERT(VARCHAR(10),@Id)  
  
  Go

    
  
  
  
Alter PROC [dbo].[usp_RetailClientUplineCommission]   
 @OriginalRTranId varchar(14) = NULL  
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
 DECLARE @ClientApiUserReferenceId varchar(30) DECLARE @TranType INT= 0    DECLARE @RetailUserId varchar(14)  
 DECLARE @ApiKey varchar(10)   DECLARE @CurrentUserType INT = 0  
    DECLARE @TelecomOperatorName varchar(30)    DECLARE @L1MarginPercent DECIMAL(10,2) = 0    DECLARE @L1MaxMargin DECIMAL(10,2) = 0  
    DECLARE @OperatorCircle varchar(50)   DECLARE @L2MarginPercent DECIMAL(10,2) = 0    DECLARE @L2MaxMargin DECIMAL(10,2) = 0  
    DECLARE @RechargeMobileNumber varchar(50) DECLARE @L3MarginPercent DECIMAL(10,2) = 0    DECLARE @L3MaxMargin DECIMAL(10,2) = 0  
    DECLARE @Amount decimal(18, 2)    DECLARE @DistributorId varchar(20) DECLARE @MasterDistributorId VARCHAR(20)  
    DECLARE @OpeningBalance decimal(18, 2)  DECLARE @DistributorMarginDiffPercent DECIMAL(10,2) DECLARE @MasterDistributorMarginDiffPercent DECIMAL(10,2)  
    DECLARE @DebitAmount decimal(18, 2)   DECLARE @DistributorMarginDiffMaxMargin DECIMAL(10,2) DECLARE @MasterDistributorMarginDiffMaxMargin DECIMAL(10,2)  
    DECLARE @CreditAmount decimal(18, 2)  DECLARE @DistributorOrderNo bigint DECLARE @MasterDistributorOrderNo bigint  
    DECLARE @ClosingBalance decimal(18, 2)  
    DECLARE @SalePrice decimal(10, 2)  
    DECLARE @CGst decimal(10, 2)  
    DECLARE @SGst decimal(10, 2)  
    DECLARE @IGst decimal(10, 2)  
    DECLARE @Margin decimal(10, 2)    DECLARE @RetailerTDS DECIMAL(10,2) = 0   DECLARE @DistributorTDS DECIMAL(10,2) = 0   DECLARE @MasterDistributorTDS DECIMAL(10,2) = 0  
 DECLARE @FixMargin decimal(10, 2)   DECLARE @L1FixMargin DECIMAL(10,2) = 0   DECLARE @L2FixMargin DECIMAL(10,2) = 0   DECLARE @L3FixMargin DECIMAL(10,2) = 0  
  DECLARE @MarginPercentUPTO200 decimal(10, 2)   DECLARE @L1MarginPercentUPTO200 DECIMAL(10,2) = 0   DECLARE @L2MarginPercentUPTO200 DECIMAL(10,2) = 0   DECLARE @L3MarginPercentUPTO200 DECIMAL(10,2) = 0 
  DECLARE @L1MarginPercentUPTO200Diff DECIMAL(10,2) = 0   DECLARE @DistributorMarginPercentUPTO200Diff DECIMAL(10,2) = 0   DECLARE @MasterDistributorMarginPercentUPTO200Diff DECIMAL(10,2) = 0 
    DECLARE @RequestIp varchar(100)    DECLARE @L1FixMarginDiff DECIMAL(10,2) = 0   DECLARE @DistributorFixMarginDiff DECIMAL(10,2) = 0   DECLARE @MasterDistributorFixMarginDiff DECIMAL(10,2) = 0  
    DECLARE @RequestTime datetime      
    DECLARE @RechargeType varchar(3)  
    DECLARE @Remarks varchar(200)  
    DECLARE @RetailUserOrderNo int  
 DECLARE @RechargeStatus varchar(10)  
 DECLARE @Gateway smallint  
 DECLARE @OurApiId varchar(10)    DECLARE @UtilityType VARCHAR(80)  
 DECLARE @RefundTransactionId varchar(14)  
 DECLARE @OtherApiId varchar(50)    DECLARE @CreateDate DATETIME   DECLARE @UPPCL_PaymentType VARCHAR(20) DECLARE @CurrentMargin DECIMAL(10,2) = 0  
  
 BEGIN TRAN  
 DECLARE @Id varchar(14);  
   
 SELECT @ClientApiUserReferenceId = ClientApiUserReferenceId, @RetailUserId = RetailUserId,  @TelecomOperatorName = TelecomOperatorName, @OperatorCircle=OperatorCircle, @OtherApiId=OtherApiId,  
 @RechargeMobileNumber = RechargeMobileNumber, @Amount = Amount, @DebitAmount = CreditAmount, @CreditAmount = DebitAmount, @SalePrice = -SalePrice, @CGst=-CGst, @SGst=-SGst, @IGst=-IGst, @Margin=-Margin,   
 @RequestIp=RequestIp, @RequestTime=RequestTime, @RechargeType=RechargeType, @RetailUserOrderNo=RetailUserOrderNo, @Gateway=Gateway, @OurApiId=OurApiId, @RefundTransactionId = RefundTransactionId, @UPPCL_PaymentType = UPPCL_PaymentType FROM RTran   
 WITH(NOLOCK) WHERE Id=@OriginalRTranId AND IsMarginDistributed IS NULL;  
  
 SELECT @UtilityType = OperatorType FROM OperatorMaster WITH(NOLOCK) WHERE OperatorName=@TelecomOperatorName;  
 PRINT('START ' + @OriginalRTranId);  
  
 UPDATE RTran SET IsMarginDistributed=1 WHERE Id=@OriginalRTranId;   
  
 IF (EXISTS(SELECT MasterId FROM RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId)) --commission diff give  
 BEGIN  
  IF(@UtilityType IN('Mobile','DTH'))  
  BEGIN  
   SELECT @L1MarginPercent = ISNULL(MarginPercent,0), @L1MaxMargin=ISNULL(MaxMargin,0) FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@RetailUserId AND OperatorName=@TelecomOperatorName;  
  END  
  ELSE  
  BEGIN  
   -- Utility Start  
   SELECT @L1MarginPercent = ISNULL(MarginPercent,0), @L1MaxMargin=ISNULL(MaxMargin,0), @L1FixMargin=ISNULL(FixMargin,0), @L1MarginPercentUPTO200 = ISNULL(MarginPercentUpto200,0) FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@RetailUserId AND UtilityType=@UtilityType;  
   IF(@Amount<=200 AND @UPPCL_PaymentType='FULL')  
   BEGIN  
    SELECT @CreditAmount = ISNULL((@Amount*@L1MarginPercentUPTO200)/100,0);;  
    SELECT @CurrentMargin = @L1MarginPercentUPTO200;  
   END   
   else IF(@Amount<=4000 AND @UPPCL_PaymentType='FULL')  
   BEGIN  
    SELECT @CreditAmount = @L1FixMargin;  
    SELECT @CurrentMargin = @L1FixMargin;  
   END  
   ELSE  
   BEGIN  
    SELECT @CreditAmount = ISNULL((@Amount*@L1MarginPercent)/100,0);  
    SELECT @CurrentMargin =  @L1MarginPercent;  
   END  
  
   IF(@CreditAmount>0)  
   BEGIN  
    SELECT @OpeningBalance = [dbo].[RetailUserBalance](@RetailUserId);  
    SELECT @DebitAmount = 0;  
  
    SELECT @RetailerTDS = (@CreditAmount * 2)/100;  
    SELECT @CreditAmount = @CreditAmount - @RetailerTDS;  
   END  
  
   SELECT @ClosingBalance = @OpeningBalance + @CreditAmount - @DebitAmount;  
   SELECT @Id = RIGHT('CD10' + convert(varchar(9), next value for RTranNextId),14);  
   SELECT @CreateDate = CURRENT_TIMESTAMP;  
   SELECT @TranType = 7;  
   SELECT @Remarks = 'Commission for business of ' + @TelecomOperatorName + ', Mobile-' + @RechargeMobileNumber +  ' of amount ' + CONVERT(VARCHAR(14),@Amount);  
  
   IF(@CreditAmount>0)  
   BEGIN  
    INSERT INTO [dbo].[RTran] ([Id], [RetailUserId], [TelecomOperatorName], [RechargeMobileNumber], [Amount], [Deduction], [FinalAmount], [OpeningBalance], [DebitAmount], [CreditAmount], [ClosingBalance],[Margin],  [TranType], [Remarks], [OtherApiId], [Extra1], [Extra2], [RequestTime], [RetailUserOrderNo], [CreateDate], [ConfirmDate], [TDS])  
    SELECT @Id, @RetailUserId, @TelecomOperatorName, @RechargeMobileNumber, @CreditAmount, NULL, @CreditAmount, @OpeningBalance, @DebitAmount, @CreditAmount, @ClosingBalance, @CreditAmount,  
    @TranType, @Remarks, @RetailUserId, @OriginalRTranId, @CurrentMargin, @RequestTime, @RetailUserOrderNo, @CreateDate, CURRENT_TIMESTAMP, @RetailerTDS  
   END  
       
  END  
  
  PRINT('Margin Detail %: ' + CONVERT(VARCHAR(10),@L1MarginPercent) +  ', Max: ' + CONVERT(VARCHAR(10),@L1MaxMargin) +  ', Fix: ' + CONVERT(VARCHAR(10),@L1FixMargin) + ', O-' + @TelecomOperatorName);  
  IF(@L1MarginPercent>-1 OR @L1FixMargin>-1 OR @L1MarginPercentUPTO200 > -1)  
  BEGIN  
   SELECT @DistributorId = MasterId FROM RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;  
   SELECT @DistributorOrderNo = OrderNo FROM RetailUser WITH(NOLOCK) WHERE Id=@DistributorId;  
   PRINT('Distributor Found: ' + @DistributorId);  
  
   SELECT @CurrentMargin = 0;  
  
   IF(@UtilityType IN('Mobile','DTH'))  
   BEGIN  
    SELECT @L2MarginPercent = ISNULL(MarginPercent,0), @L2MaxMargin=ISNULL(MaxMargin,0)  FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@DistributorId AND OperatorName=@TelecomOperatorName;  
   END  
   ELSE  
   BEGIN  
    SELECT @L2MarginPercent = ISNULL(MarginPercent,0), @L2MaxMargin=ISNULL(MaxMargin,0), @L2FixMargin=ISNULL(FixMargin,0), @L2MarginPercentUPTO200 = ISNULL(MarginPercentUpto200,0) FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@DistributorId AND UtilityType=@UtilityType;  
   END  
  
   SELECT @DistributorMarginDiffPercent = @L2MarginPercent-@L1MarginPercent; SELECT @DistributorMarginDiffMaxMargin = @L2MaxMargin - @L1MaxMargin;  
   SELECT @DistributorFixMarginDiff = @L2FixMargin-@L1FixMargin;
   Select @DistributorMarginPercentUPTO200Diff = @L2MarginPercentUPTO200 - @L1MarginPercentUPTO200;
  
   IF(@UtilityType IN('Mobile','DTH'))  
   BEGIN  
    SELECT @DistributorMarginDiffMaxMargin=200;  
   END  
  
   PRINT('Distributor Diff %: ' + CONVERT(VARCHAR(10),@DistributorMarginDiffPercent) +  ', Distributor Diff: ' + CONVERT(VARCHAR(10),@DistributorMarginDiffMaxMargin) + ', O-' + @UtilityType);  
  
   IF((@DistributorMarginDiffPercent>=0 AND @DistributorMarginDiffMaxMargin>=0) OR @DistributorFixMarginDiff>=0 OR @DistributorMarginPercentUPTO200Diff>=0)  
   BEGIN  
    --Give difference margin to distributor  
    SELECT @OpeningBalance = [dbo].[RetailUserBalance](@DistributorId);  
    SELECT @DebitAmount = 0; 
	IF(@Amount<=200 AND @UPPCL_PaymentType='FULL')  
    BEGIN  
     SELECT @CreditAmount = ISNULL((@Amount*@DistributorMarginPercentUPTO200Diff)/100,0);;  
     SELECT @CurrentMargin = @DistributorMarginPercentUPTO200Diff;  
    END 
    ELSE IF(@Amount<=4000 AND @UPPCL_PaymentType='FULL')  
    BEGIN  
     SELECT @CreditAmount = @DistributorFixMarginDiff;  
     SELECT @CurrentMargin = @DistributorFixMarginDiff;  
    END  
    ELSE  
    BEGIN  
     SELECT @CreditAmount = ISNULL((@Amount*@DistributorMarginDiffPercent)/100,0);  
     SELECT @CurrentMargin = @DistributorMarginDiffPercent;  
    END  
      
                IF(@UtilityType NOT IN('Mobile','DTH'))  
                BEGIN  
     IF(@Amount>2000)  
     BEGIN  
      IF(@CreditAmount>@DistributorMarginDiffMaxMargin AND @DistributorMarginDiffMaxMargin>0)  
      BEGIN  
       SELECT @CreditAmount = @DistributorMarginDiffMaxMargin;  
      END  
     END  
                END  
  
    SELECT @DistributorTDS = (@CreditAmount * 2)/100;  
    SELECT @CreditAmount = @CreditAmount - @DistributorTDS;  
  
    IF(@CreditAmount>0)  
    BEGIN  
     SELECT @Id = RIGHT('CD10' + convert(varchar(9), next value for RTranNextId),14);  
     SELECT @ClosingBalance = @OpeningBalance + @CreditAmount - @DebitAmount;  
     SELECT @CreateDate = CURRENT_TIMESTAMP;  
     SELECT @TranType = 7;  
     SELECT @Remarks = 'Commission for business of ' + @TelecomOperatorName + ', Mobile-' + @RechargeMobileNumber +  ' of amount ' + CONVERT(VARCHAR(14),@Amount) + '. Retailer-' + CONVERT(VARCHAR(10),@RetailUserOrderNo);  
     PRINT('Distributor Commission: ' + @DistributorId + ', ' + @Remarks);  
  
     INSERT INTO [dbo].[RTran] ([Id], [RetailUserId], [TelecomOperatorName], [RechargeMobileNumber], [Amount], [Deduction], [FinalAmount], [OpeningBalance], [DebitAmount], [CreditAmount], [ClosingBalance],  
     [Margin],  [TranType], [Remarks], [OtherApiId], [Extra1], [Extra2], [RequestTime], [RetailUserOrderNo], [CreateDate], [ConfirmDate], [TDS])  
     SELECT @Id, @DistributorId, @TelecomOperatorName, @RechargeMobileNumber, @CreditAmount, NULL, @CreditAmount, @OpeningBalance, @DebitAmount, @CreditAmount, @ClosingBalance, @CreditAmount,  
      @TranType, @Remarks, @RetailUserId, @OriginalRTranId, @CurrentMargin, @RequestTime, @DistributorOrderNo,@CreateDate, CURRENT_TIMESTAMP, @DistributorTDS  
    END  
  
    IF (EXISTS(SELECT MasterId FROM RetailUser WITH(NOLOCK) WHERE Id=@DistributorId))  
    BEGIN  
     SELECT @MasterDistributorId = MasterId FROM RetailUser WITH(NOLOCK) WHERE Id=@DistributorId;  
     SELECT @MasterDistributorOrderNo = OrderNo FROM RetailUser WITH(NOLOCK) WHERE Id=@MasterDistributorId;  
  
     SELECT @CurrentMargin = 0;  
  
     PRINT('MasterDistributor Found: ' + @DistributorId);  
  
     IF(@UtilityType IN('Mobile','DTH'))  
     BEGIN  
      SELECT @L3MarginPercent = ISNULL(MarginPercent,0), @L3MaxMargin=ISNULL(MaxMargin,0) FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@MasterDistributorId AND OperatorName=@TelecomOperatorName;  
     END  
     ELSE  
     BEGIN  
      SELECT @L3MarginPercent = ISNULL(MarginPercent,0), @L3MaxMargin=ISNULL(MaxMargin,0), @L3FixMargin=ISNULL(FixMargin,0), @L3MarginPercentUPTO200=ISNULL(MarginPercentUpto200,0) FROM UtilityMargin WITH(NOLOCK) WHERE RetailUserid=@MasterDistributorId AND UtilityType=@UtilityType;  
     END  
       
     SELECT @MasterDistributorMarginDiffPercent = @L3MarginPercent-@L2MarginPercent; SELECT @MasterDistributorMarginDiffMaxMargin = @L3MaxMargin - @L2MaxMargin;  
     SELECT @MasterDistributorFixMarginDiff = @L3FixMargin-@L2FixMargin;  SELECT @MasterDistributorMarginPercentUPTO200Diff = @L3MarginPercentUPTO200-@L2MarginPercentUPTO200; 
  
     IF(@UtilityType IN('Mobile','DTH'))  
     BEGIN  
      SELECT @MasterDistributorMarginDiffMaxMargin=200;  
     END  
  
     PRINT('MasterDistributor Diff %: ' + CONVERT(VARCHAR(10),@MasterDistributorMarginDiffPercent) +  ', MasterDistributor Diff: ' + CONVERT(VARCHAR(10),@MasterDistributorMarginDiffMaxMargin) + ', O-' + @UtilityType);  
  
     IF((@MasterDistributorMarginDiffPercent>0 AND @MasterDistributorMarginDiffMaxMargin>0) OR @MasterDistributorFixMarginDiff>0 OR @MasterDistributorMarginPercentUPTO200Diff>0)  
     BEGIN  
      --Give difference margin to master distributor  
      SELECT @OpeningBalance = [dbo].[RetailUserBalance](@MasterDistributorId);  
      SELECT @DebitAmount = 0; 
	  IF(@Amount<=200 AND @UPPCL_PaymentType='FULL')  
      BEGIN  
       SELECT @CreditAmount = ISNULL((@Amount*@MasterDistributorMarginPercentUPTO200Diff)/100,0);;  
       SELECT @CurrentMargin = @MasterDistributorMarginPercentUPTO200Diff;  
      END 
      ELSE IF(@Amount<=4000 AND @UPPCL_PaymentType='FULL')  
      BEGIN  
       SELECT @CreditAmount = @MasterDistributorFixMarginDiff;  
       SELECT @CurrentMargin = @MasterDistributorFixMarginDiff;  
      END  
      ELSE  
      BEGIN  
       SELECT @CreditAmount = ISNULL((@Amount*@MasterDistributorMarginDiffPercent)/100,0);  
       SELECT @CurrentMargin = @MasterDistributorMarginDiffPercent;  
      END  
        
                          
                        IF(@UtilityType NOT IN('Mobile','DTH'))  
                        BEGIN  
       IF(@Amount>2000)  
       BEGIN  
        IF(@CreditAmount>@MasterDistributorMarginDiffMaxMargin AND @MasterDistributorMarginDiffMaxMargin>0)  
        BEGIN  
         SELECT @CreditAmount = @MasterDistributorMarginDiffMaxMargin;  
        END  
       END  
                        END  
  
      SELECT @MasterDistributorTDS = (@CreditAmount * 2)/100;  
      SELECT @CreditAmount = @CreditAmount - @MasterDistributorTDS;  
        
      IF(@CreditAmount>0)  
      BEGIN  
       SELECT @Id = RIGHT('CM10' + convert(varchar(9), next value for RTranNextId),14);  
       SELECT @ClosingBalance = @OpeningBalance + @CreditAmount - @DebitAmount;  
       SELECT @CreateDate = CURRENT_TIMESTAMP;  
       SELECT @TranType = 7;  
       SELECT @Remarks = 'Commission for business of ' + @TelecomOperatorName + ', Mobile-' + @RechargeMobileNumber +  ' of amount ' + CONVERT(VARCHAR(14),@Amount) + '. Distributor-' + CONVERT(VARCHAR(10),@DistributorOrderNo);  
       PRINT('Master Distributor Commission: ' + @MasterDistributorId + ', ' + @Remarks);  
  
       INSERT INTO [dbo].[RTran] ([Id], [RetailUserId], [TelecomOperatorName], [RechargeMobileNumber], [Amount], [Deduction], [FinalAmount], [OpeningBalance], [DebitAmount], [CreditAmount], [ClosingBalance],  
       [Margin],  [TranType], [Remarks], [OtherApiId], [Extra1], [Extra2], [RequestTime], [RetailUserOrderNo], [CreateDate], [ConfirmDate], [TDS])  
       SELECT @Id, @MasterDistributorId, @TelecomOperatorName, @RechargeMobileNumber, @CreditAmount, NULL, @CreditAmount, @OpeningBalance, @DebitAmount, @CreditAmount, @ClosingBalance, @CreditAmount,  
        @TranType, @Remarks, @DistributorId, @OriginalRTranId, @CurrentMargin, @RequestTime, @MasterDistributorOrderNo, @CreateDate, CURRENT_TIMESTAMP, @MasterDistributorTDS  
      END  
     END   
    END  
   END  
  END  
 END         
  
 COMMIT  
  
  Go

    
  
Alter PROC [dbo].[usp_RetailClientMarginSheetByOrderNo]   
    @RetailUserOrderNo bigint  
AS   
 BEGIN  
  DECLARE @RetailUserId VARCHAr(10);  
        SELECT @RetailUserId = Id FROM RetailUser WITh(NOLOCK) WHERE OrderNo=@RetailUserOrderNo;  
        SELECT ID, [dbo].[RetailerNamewithIdMobile](RetailUserid) AS RetailUserName, UtilityType, OperatorName, FixMargin, MarginPercent, MaxMargin, MaxDailyLimit, Active, MarginPercentUpto200  FROM UtilityMargin   
  WITH(NOLOCK)   
        WHERE RetailUserid = @RetailUserId  
        ORDER BY RetailUserid,OperatorName  
 END  

 Go

   
  
Alter PROC [dbo].[usp_RetailClientMarginSheetByOrderNoParentValidation]   
    @RetailUserOrderNo bigint, @ParentId varchar(10)  
AS   
 BEGIN  
  DECLARE @RetailUserId VARCHAr(10);  
        SELECT @RetailUserId = Id FROM RetailUser WITh(NOLOCK) WHERE OrderNo=@RetailUserOrderNo AND MasterId=@ParentId;  
        SELECT ID, [dbo].[RetailerNamewithIdMobile](RetailUserid) AS RetailUserName, UtilityType, OperatorName, FixMargin, MarginPercent, MaxMargin, MaxDailyLimit, Active, MarginPercentUpto200  FROM UtilityMargin   
        WITH(NOLOCK)   
        WHERE RetailUserid = @RetailUserId  
        ORDER BY RetailUserid,OperatorName  
 END  
   
   Go

     
  
Alter PROC [dbo].[usp_RetailClientMarginSheetDownlineByClientId]   
    @RetailUserId varchar(14)  
AS   
 BEGIN  
  SELECT ID, [dbo].[RetailerNamewithId](RetailUserid) AS RetailUserid, UtilityType, OperatorName, FixMargin, MarginPercent, MaxMargin, MaxDailyLimit, Active, MarginPercentUpto200  FROM UtilityMargin   
  WITH(NOLOCK) WHERE RetailUserId IN(SELECT Id FROM RetailUser WHERE MasterId=@RetailUserId) ORDER BY OperatorName  
 END 
 
 Go

   
  
Alter PROC [dbo].[usp_RetailClientMarginSheetByClientId]   
    @RetailUserId varchar(14)  
AS   
 BEGIN  
  SELECT ID, RetailUserid, UtilityType, OperatorName, FixMargin, MarginPercent, MaxMargin, MaxDailyLimit, Active, MarginPercentUpto200 FROM UtilityMargin   
  WITH(NOLOCK) WHERE RetailUserId=@RetailUserId ORDER BY OperatorName  
 END  
   
 Go
 
   
  
Alter PROC [dbo].[usp_RetailClientMarginSheetList]   
    @UserId varchar(14)  
AS   
 BEGIN  
  SELECT ID, [dbo].[RetailerNamewithIdMobile](RetailUserid) AS RetailUserid, UtilityType, OperatorName, FixMargin, MarginPercent, MaxMargin, MaxDailyLimit, Active, MarginPercentUpto200  FROM UtilityMargin   
  WITH(NOLOCK) ORDER BY RetailUserid,OperatorName  
 END  
   
  
   
  
  
  
   
  
  
  