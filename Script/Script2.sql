Alter Table RetailUser 
Add KYCRequired tinyint

Alter Table [dbo].[RetailUser]
Add PhysicalKYCDone tinyint

Alter Table [dbo].[RetailUser]
Add ActivatedOn datetime

Go

Alter PROC [dbo].[usp_RetailUserList] 
AS 
SELECT RU.Id, UserType, MarginType, [dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName, Mobile AS MobileNumber,  
City, EMail, MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, [Address], OrderNo AS USL, Active, LoginTime, KYCRequired, PhysicalKYCDone, ActivatedOn
FROM RetailUser RU WITH(NOLOCK)
Left Join UserLoginTimeInfo ULTI on ULTI.RetailUserId = RU.ID and CAST(LoginTime as date) = CAST(getdate() as date)
ORDER BY OrderNo ASC

Go


 Alter PROC [dbo].[usp_updateUserActivationState] 
    @RetailUserId varchar(14)
AS 
	DECLARE @Active tinyint 
	Declare @OperationMessage nvarchar(500)
	DECLARE @RetailUserOrderNo INT
	SELECT @Active = Active, @RetailUserOrderNo = OrderNo from RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;
	if(@Active = 1)
	begin 
	   Set @Active = 0
    end
	else
	begin
	   Set @Active = 1
    end
	UPDATE RetailUser SET [Active]=@Active, KYCRequired = 0 WHERE Id=@RetailUserId;
	if(@Active = 1)
	begin 
	   SELECT @OperationMessage =  'Success: user ' + CONVERT(VARCHAR(20),@RetailUserOrderNo) + ' activated.'
    end
	else
	begin
	   SELECT @OperationMessage =  'Success: user ' + CONVERT(VARCHAR(20),@RetailUserOrderNo) + ' deactivated.'
    end
	SELECT @OperationMessage as OperationMessage;
	
Go

  
Alter PROC [dbo].[usp_RechargeReportAllRetailClientAdminByDate]   
    @TranDateFrom DATE, @TranDateTo DATE  
AS   
 --DECLARE @RetailUserId varchar(14)  
 Select RTran.Id as Rid, RetailUserId, [dbo].[RetailClientOrderNoUserName](RetailUserId) AS RetailerDetail, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, RefundTransactionId as RefundId, TelecomOperatorName as OperatorName, OperatorCircle,Amount,   
 DebitAmount as Debit, CreditAmount as Credit,Margin, OpeningBalance as OB, ClosingBalance as CB,RechargeMobileNumber as RechargeNumber, RechargeStatus, ISNULL(Remarks,'') as Remarks, CreateDate, LiveId, InitialResponseData,  
 [dbo].[TranTypeString](TranType) AS TransactionType, UPPCL_PaymentType AS PaymentType from [RTran] WITH(NOLOCK) inner join [dbo].[RetailUser]  on  RTran.RetailUserId = RetailUser.Id 
 WHERE (CONVERT(DATE,CreateDate) BETWEEN @TranDateFrom AND @TranDateTo) ORDER BY CreateDate 


Go



Create PROC [dbo].[usp_updateKYCState] 
    @RetailUserId varchar(14),
	@Active tinyint,
	@PhysicalKYCDone tinyint
AS 
	Declare @OperationMessage nvarchar(500)
	DECLARE @RetailUserOrderNo INT
	SELECT  @RetailUserOrderNo = OrderNo from RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;
	
	UPDATE RetailUser 
	   SET [Active]=@Active, 
	       KYCRequired = case when @Active = 1 then 0 else 1 end,
		   PhysicalKYCDone = @PhysicalKYCDone,
		   ActivatedOn = case when @PhysicalKYCDone = 1 then null else GETDATE() end
		   WHERE Id=@RetailUserId;
	SELECT @OperationMessage =  'Success: user activation state updated.'
	SELECT @OperationMessage as OperationMessage;
	
Go

Alter PROC [dbo].[usp_RetailUserInsert]   
    @MasterId varchar(10) = NULL,  
    @UserType int = NULL,  
    @MarginType smallint = NULL,  
    @FirstName nvarchar(50),  
    @MiddleName nvarchar(50) = NULL,  
    @LastName nvarchar(50) = NULL,  
    @EMail nvarchar(300) = NULL,  
    @Gender varchar(50) = NULL,  
    @DateOfBirth datetime = NULL,  
    @Mobile varchar(30) = NULL,  
    @Address varchar(400) = NULL,  
    @City varchar(100) = NULL,  
    @PinCode varchar(50) = NULL,  
    @Country varchar(100) = NULL,  
    @StateName varchar(150) = NULL,        
    @Password varchar(400) = NULL,      
    @CreditLimit float = NULL,  
    @MinFundValue float = NULL,  
    @Commission float = NULL,      
    @Active int = NULL,  
    @SignupDate datetime = NULL  
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
   
 BEGIN TRAN  
   
 DECLARE @Id varchar(10) DECLARE @UserCount smallint = 0 DECLARE @EmailCount smallint = 0 DECLARE @OperationMessage varchar(250);  
 SELECT @UserCount = ISNULL(COUNT(Id),0) from RetailUser WITH(NOLOCK) WHERE Mobile=@Mobile;  
 IF @EMail IS NOT NULL  
 BEGIN  
  IF LEN(@EMail) > 0  
  BEGIN  
   SELECT @EmailCount = ISNULL(COUNT(Id),0) from RetailUser WITH(NOLOCK) WHERE EMail=@EMail;     
  END  
 END  
  
 IF @UserCount>0  
 BEGIN  
  SELECT @OperationMessage = 'Error: ' + @Mobile + ' number already exist in the system, please try wth different mobile number.';  
 END  
 ELSE IF @EmailCount>0  
 BEGIN  
  SELECT @OperationMessage = 'Error: ' + @EMail + ' email already exist in the system, please try wth different email.';    
 END  
 ELSE  
 BEGIN  
  SELECT @Id = RIGHT('RU00' + convert(nvarchar(10), next value for RetailUserNextId),10);  
  
  INSERT INTO [dbo].[RetailUser] ([Id], [MasterId], [UserType], [MarginType], [FirstName], [MiddleName], [LastName], [EMail], [Gender], [DateOfBirth],   
  [Mobile], [Address], [City], [PinCode], [Country], [StateName], [Password], [CreditLimit],   
  [MinFundValue], [Commission], [Active],  [SignupDate], KYCRequired, PhysicalKYCDone)  
  SELECT @Id, @MasterId, @UserType, @MarginType, @FirstName, @MiddleName, @LastName, @EMail, @Gender, @DateOfBirth, @Mobile, @Address,  
  @City, @PinCode, @Country, @StateName, @Password, @CreditLimit, @MinFundValue, @Commission,   
  0, CURRENT_TIMESTAMP, 1, 0;  
    
  SELECT @OperationMessage = 'Successfully created user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';  
  
  DECLARE @Smstemplate nvarchar(2000);  
  SELECT TOP 1 @Smstemplate = Smstemplate from SmsTemplate where SmsType='welcome' AND RetailUserId IS NULL;  
  IF @Smstemplate IS NOT NULL  
  BEGIN  
   SELECT @Smstemplate = REPLACE(@SmsTemplate,'<name>',@FirstName + ' ' + @MiddleName + ' ' + @LastName);  
   SELECT @Smstemplate = REPLACE(@SmsTemplate,'<loginid>',@Mobile);  
   SELECT @Smstemplate = REPLACE(@SmsTemplate,'<password>',@Password);  
   INSERT INTO OutSms(MobileNumber,[Message], CreateDate) VALUES (@Mobile, @Smstemplate, GETDATE());  
   SELECT @OperationMessage = @OperationMessage + ' Welcome message sent to user mobile.'  
  END  
 END  
  
 SELECT @OperationMessage AS OperationMessage, @Id as Id;   
                 
 COMMIT  

 Go
    
    
    
Alter PROC [dbo].[usp_RetailClient_ValidateLogin]     
    @MobileNumber varchar(20),    
    @Password varchar(50),    
 @FToken varchar(400)=NULL,    
 @Did varchar(400)=NULL,    
 @OutResult int=0 OUT    
AS     
 SET NOCOUNT ON     
 SET XACT_ABORT ON      
     
 BEGIN TRAN    
 declare @Id varchar(10)  
 SELECT @OutResult = ISNULL(OrderNo,0), @Id = Id from [RetailUser] with(nolock) WHERE [Mobile] = @MobileNumber AND [Password]=@Password AND Active=1;    
     
 --UPDATE RetailUser SET AndroidFcmToken=@FToken WHERE OrderNo=@OutResult;    
 DECLARE @CurrentFtoken VARCHAR(400) = '';    
 IF(@OutResult > 0 AND @FToken IS NOT NULL)    
 BEGIN    
  SELECT @CurrentFtoken = ISNULL(AndroidFcmToken,'') FROm RetailUser WITh(NOLOCK) WHERE OrderNo=@OutResult;    
  IF(@CurrentFtoken <> @FToken)    
  BEGIN    
   UPDATE RetailUser SET AndroidFcmToken=@FToken WHERE OrderNo=@OutResult;    
  END    
 END    
    
 --UPDATE RetailUser SET AndroidUuid=@Did WHERE OrderNo=@OutResult;    
 DECLARE @CurrentDid VARCHAR(400) = '';    
 IF(@OutResult > 0 AND @Did IS NOT NULL)    
 BEGIN    
  SELECT @CurrentDid = ISNULL(AndroidUuid,'') FROm RetailUser WITh(NOLOCK) WHERE OrderNo=@OutResult;    
  IF(@CurrentDid <> @Did)    
  BEGIN    
   UPDATE RetailUser SET AndroidUuid=@Did WHERE OrderNo=@OutResult;    
  END    
 END    
 declare @ActivatedOn datetime
 declare @days int
 SELECT @ActivatedOn = ActivatedOn FROm RetailUser WITh(NOLOCK) WHERE OrderNo=@OutResult; 
 if(@ActivatedOn is not null)
 begin
	 SELECT @days = DATEDIFF(day, GetDate(), @ActivatedOn)
	 if(@days > 10)
	 begin
	  UPDATE RetailUser SET Active=0,@ActivatedOn = null, KYCRequired = 1  WHERE OrderNo=@OutResult; 
	 end
 end
 
 SELECT Id, UserType, MarginType, ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName,'') + ' ' + ISNULL(LastName,'') AS RetailerName, Mobile AS MobileNumber,     
  City, EMail, MasterId as Parent, [Address], OrderNo AS USL, Active, DefaultUtilityOperator, ActivatedOn   
  FROM RetailUser WITH(NOLOCK) WHERE OrderNo=@OutResult;    
  
  if((Select Count(1) from [dbo].[UserLoginTimeInfo] where CAST(LoginTime as date) = CAST(getdate() as date)) = 0)  
  begin  
      insert into [dbo].[UserLoginTimeInfo](RetailUserId, LoginTime) values(@Id, getdate())  
  end  
 COMMIT  
 
Go



  
Create PROC [dbo].[usp_RetailUserUpdate] 
    @UserId varchar(10) = null,
    @MarginType smallint = NULL,  
    @FirstName nvarchar(50),  
    @MiddleName nvarchar(50) = NULL,  
    @LastName nvarchar(50) = NULL,  
    @EMail nvarchar(300) = NULL,  
    @Gender varchar(50) = NULL,  
    @DateOfBirth datetime = NULL,  
    @Mobile varchar(30) = NULL,  
    @Address varchar(400) = NULL,  
    @City varchar(100) = NULL,  
    @PinCode varchar(50) = NULL,  
    @Country varchar(100) = NULL,  
    @StateName varchar(150) = NULL,        
    @Password varchar(400) = NULL,      
    @CreditLimit float = NULL,  
    @MinFundValue float = NULL,  
    @Commission float = NULL,      
    @Active int = NULL,  
    @KYCRequired int = NULL,
	@PhysicalKYCDone int = null
AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
   
 BEGIN TRAN  
   
 DECLARE @Id varchar(10) DECLARE @UserCount smallint = 0 DECLARE @EmailCount smallint = 0 DECLARE @OperationMessage varchar(250);  
 SELECT @UserCount = ISNULL(COUNT(Id),0) from RetailUser WITH(NOLOCK) WHERE Mobile=@Mobile and Id <> @UserId;  
 IF @EMail IS NOT NULL  
 BEGIN  
  IF LEN(@EMail) > 0  
  BEGIN  
   SELECT @EmailCount = ISNULL(COUNT(Id),0) from RetailUser WITH(NOLOCK) WHERE EMail=@EMail and Id <> @UserId;     
  END  
 END  
  
 IF @UserCount>0  
 BEGIN  
  SELECT @OperationMessage = 'Error: ' + @Mobile + ' number already exist in the system, please try wth different mobile number.';  
 END  
 ELSE IF @EmailCount>0  
 BEGIN  
  SELECT @OperationMessage = 'Error: ' + @EMail + ' email already exist in the system, please try wth different email.';    
 END  
 ELSE  
 BEGIN  
   
  
  Update [dbo].[RetailUser] 
  Set 
  [MarginType] = @MarginType, 
  [FirstName] = @FirstName, 
  [MiddleName] = @MiddleName, 
  [LastName] = @LastName, 
  [EMail] = @EMail, 
  [Gender] = @Gender,
  [DateOfBirth] = @DateOfBirth,   
  [Mobile] = @Mobile,
  [Address] = @Address,
  [City] = @City,
  [PinCode] = @PinCode,
  [Country] = @Country, 
  [StateName] = @StateName,
  [Password] = @Password,
  [CreditLimit] = @CreditLimit,   
  [MinFundValue] = @MinFundValue,
  [Commission]= @Commission,
  [Active] = @Active,
  [KYCRequired] = @KYCRequired,
  [PhysicalKYCDone] = @PhysicalKYCDone
  Where Id = @UserId
    
  SELECT @OperationMessage = 'Successfully updated user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';  
  
 END  
  
 SELECT @OperationMessage AS OperationMessage;   
                 
 COMMIT

Go
    
Create PROC [dbo].[usp_GetUserDetailsToUpdate]     
@Id nvarchar(10)    
AS     
SELECT     
Id,    
MasterId,     
UserType,    
MarginType,    
FirstName,     
MiddleName,    
LastName,    
Email,    
Gender,    
DateOfBirth,    
Mobile,      
Address,    
City,    
StateName,    
PinCode,    
Password,    
Commission,    
Active,
[KYCRequired],
[PhysicalKYCDone]
FROM RetailUser WITH(NOLOCK)    
where Id = @Id  






