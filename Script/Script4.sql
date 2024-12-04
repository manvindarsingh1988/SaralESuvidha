CREATE TABLE [dbo].[KYCToken](
	[Token] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
	[Name] [nvarchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

Go

Insert into [KYCToken]([Token], [CreatedOn], [Name]) values('key_live_xVAQRnIirEcjhociT9OJXfx7LWr9EUIc',null, 'x-api-key')
Insert into [KYCToken]([Token], [CreatedOn], [Name]) values('1.0',null, 'x-api-version')
Insert into [KYCToken]([Token], [CreatedOn], [Name]) values('secret_live_kzXWRlGM829SVvAePHrdPMFwWneKOlUq',null, 'x-api-secret')

Go

create Procedure usp_GetKYCTokens
as
Begin
Select * from [dbo].[KYCToken]
end

Go

create Procedure usp_InsertOrUpdateKYCToken
@Token nvarchar(max),
@CreatedOn datetime
as
Begin
declare @count int
Select @count = Count(1) from [dbo].[KYCToken] where [Name] = 'Authorization'
if(@count = 1)
begin
update [KYCToken] set Token = @Token, CreatedOn = @CreatedOn where [Name] = 'Authorization'
end
else
begin
insert into [KYCToken](Token, CreatedOn, [Name]) values (@Token, @CreatedOn, 'Authorization')
end
end

Go

Alter Table [dbo].[RetailUser] 
Add ParmanentAddress nvarchar(500)

Alter Table [dbo].[RetailUser] 
Add CounterLocation nvarchar(500)

EXEC sp_rename '[RetailUser].KYCRequired',  'DocVerification', 'COLUMN';

Alter Table [dbo].[RetailUser] 
Add DocVerificationFailed tinyint

Alter Table [dbo].[RetailUser] 
Add FailureReason nvarchar(500)

GO

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
    @SignupDate datetime = NULL,  
    @DistributorType varchar(50) = NULL,
	@ParmanentAddress nvarchar(500) = null,
	@CounterLocation nvarchar(500) = null
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
  [MinFundValue], [Commission], [Active],  [SignupDate], DocVerification, PhysicalKYCDone, DistributorType, ParmanentAddress, CounterLocation, DocVerificationFailed)    
  SELECT @Id, @MasterId, @UserType, @MarginType, @FirstName, @MiddleName, @LastName, @EMail, @Gender, @DateOfBirth, @Mobile, @Address,    
  @City, @PinCode, @Country, @StateName, @Password, @CreditLimit, @MinFundValue, @Commission,     
  0, CURRENT_TIMESTAMP, 0, 1, @DistributorType, @ParmanentAddress, @CounterLocation, 0;    
      
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
 declare @ActivatedTill datetime    
 declare @days int    
 SELECT @ActivatedTill = ActivatedTill FROm RetailUser WITh(NOLOCK) WHERE OrderNo=@OutResult;     
 if(@ActivatedTill is not null)    
 begin    
  if(@ActivatedTill < GetDate())    
  begin    
   UPDATE RetailUser SET Active=0,ActivatedTill = null  WHERE OrderNo=@OutResult;     
  end    
 end    
     
 SELECT Id, UserType, MarginType, ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName,'') + ' ' + ISNULL(LastName,'') AS RetailerName, Mobile AS MobileNumber,         
  City, EMail, MasterId as Parent, [Address], OrderNo AS USL, Active, DefaultUtilityOperator, ActivatedTill       
  FROM RetailUser WITH(NOLOCK) WHERE OrderNo=@OutResult;        
      
  if((Select Count(1) from [dbo].[UserLoginTimeInfo] where CAST(LoginTime as date) = CAST(getdate() as date)) = 0)      
  begin      
      insert into [dbo].[UserLoginTimeInfo](RetailUserId, LoginTime) values(@Id, getdate())      
  end   
    if(@OutResult is null)    
	 begin
	    SELECT @OutResult = ISNULL(OrderNo,0), @Id = Id from [RetailUser] with(nolock) WHERE [Mobile] = @MobileNumber AND [Password]=@Password;
		IF(@OutResult > 0)        
        BEGIN 
			set @OutResult = -1
		end
	 end
 COMMIT 

Go

 Alter PROC [dbo].[usp_RetailUserList]   
AS   
SELECT RU.Id, UserType, MarginType, [dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName, Mobile AS MobileNumber,    
City, EMail, MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, [Address], OrderNo AS USL, Active, LoginTime, DocVerification, PhysicalKYCDone, ActivatedTill, DocVerificationFailed 
FROM RetailUser RU WITH(NOLOCK)  
Left Join UserLoginTimeInfo ULTI on ULTI.RetailUserId = RU.ID and CAST(LoginTime as date) = CAST(getdate() as date)  
ORDER BY OrderNo ASC

Go

Update [RetailUser] set DocVerification = 1 where Active = 1 and ActivatedTill is null and DocVerification = 0

Go

Create Proc usp_GetActivationDetails
@Id varchar(10)
As 
Select * from RetailUser where Id = @Id

Alter PROC [dbo].[usp_updateKYCState]   
    @RetailUserId varchar(14),  
 @Active tinyint,  
 @DocVerification tinyint, 
 @DocVerificationFailed tinyint,
 @FailureReason nvarchar(1000)
AS   
 Declare @OperationMessage nvarchar(500)  
 DECLARE @RetailUserOrderNo INT  
 SELECT  @RetailUserOrderNo = OrderNo from RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;  
   
 UPDATE RetailUser   
    SET [Active]=@Active,   
        DocVerification = case when @Active = 0 then 0 else @DocVerification end,  
        DocVerificationFailed = @DocVerificationFailed,  
        FailureReason = @FailureReason,
		ActivatedTill = case when @Active = 1 and @DocVerification = 0 then DATEADD(day, 10, GETDATE()) else null end 
     WHERE Id=@RetailUserId;  
 SELECT @OperationMessage =  'Success: user activation state updated.'  
 SELECT @OperationMessage as OperationMessage;

Go
   
Alter PROC [dbo].[usp_RetailUserListByParent]   
@MasterId varchar(20)  
AS   
SELECT Id, [dbo].[UserTypeString](UserType) AS UserTypeString, MarginType, FirstName + ' ' + [dbo].[RetailClientOrderNoUserNameMobile](Id) AS RetailerName, Mobile AS MobileNumber,   
City, EMail, [Address], OrderNo AS USL, Active, [dbo].[RetailUserBalance](Id) AS Balance, DocVerificationFailed -- MasterId as Parent,   
FROM RetailUser WITH(NOLOCK) WHERE MasterId=@MasterId ORDER BY OrderNo ASC  

Go

Alter PROC [dbo].[usp_GetUserDetailsToUpdate]       
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
DistributorType,
FailureReason,
ParmanentAddress,
CounterLocation
FROM RetailUser WITH(NOLOCK)      
where Id = @Id 

Go

Create Procedure usp_UpdateDocumentUploadStatus
@Id varchar(10)
As
Update [dbo].[RetailUser] set DocVerificationFailed = 0, FailureReason = null where Id = @Id 

Go

Alter PROC [dbo].[usp_updateUserActivationState]   
    @RetailUserId varchar(14),  
 @updateChild tinyint  
AS   
 DECLARE @Active tinyint   
 Declare @OperationMessage nvarchar(500)  
 DECLARE @RetailUserOrderNo INT  
 Declare @UserType int  
 SELECT @Active = Active, @RetailUserOrderNo = OrderNo, @UserType = UserType from RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;  
 if(@Active = 1)  
 begin   
    Set @Active = 0  
    end  
 else  
 begin  
    Set @Active = 1  
    end  
 UPDATE RetailUser SET [Active]=@Active WHERE Id=@RetailUserId;  
 if(@updateChild = 1 and (@UserType = 6 or @UserType = 7))  
 begin  
  if(@Active = 0)  
  begin   
    UPDATE RetailUser SET [Active]=@Active WHERE MasterId= @RetailUserId  
    if (@UserType = 7)  
    begin  
     UPDATE RetailUser SET [Active]=@Active WHERE MasterId in (Select Id from RetailUser where MasterId= @RetailUserId)  
       end  
  end  
  else  
  begin  
    UPDATE RetailUser SET [Active]= case when (DocVerification = 0 or ActivatedTill is not null) then 0 else  @Active end,  
    ActivatedTill = null  
    WHERE MasterId= @RetailUserId  
    if (@UserType = 7)  
    begin  
     UPDATE RetailUser SET [Active]= case when (DocVerification = 0 or ActivatedTill is not null) then 0 else  @Active end,  
    ActivatedTill = null   
  WHERE MasterId in (Select Id from RetailUser where MasterId= @RetailUserId)  
       end  
  end  
 end  
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

 Alter PROC [dbo].[usp_RetailUserUpdate]   
    @UserId varchar(10) = null,
    @Mobile varchar(30) = NULL,
	@Country varchar(100) = NULL, 
	@StateName varchar(150) = NULL,
	@City varchar(100) = NULL,
	@PinCode varchar(50) = NULL,
    @Address varchar(400) = NULL, 
    @CounterLocation nvarchar(1000) = NULL, 
    @DistributorType varchar(50) = NULL  
AS     
 SET NOCOUNT ON     
 SET XACT_ABORT ON      
     
 BEGIN TRAN    
     
 DECLARE @Id varchar(10) DECLARE @UserCount smallint = 0 DECLARE @EmailCount smallint = 0 DECLARE @OperationMessage varchar(250);    
 SELECT @UserCount = ISNULL(COUNT(Id),0) from RetailUser WITH(NOLOCK) WHERE Mobile=@Mobile and Id <> @UserId;    
  
    
 IF @UserCount>0    
 BEGIN    
  SELECT @OperationMessage = 'Error: ' + @Mobile + ' number already exist in the system, please try wth different mobile number.';    
 END     
 ELSE    
 BEGIN    
     
    
  Update [dbo].[RetailUser]   
  Set     
  [Mobile] = @Mobile,  
  [Address] = @Address,  
  [City] = @City,  
  [PinCode] = @PinCode,  
  [Country] = @Country,   
  [StateName] = @StateName,  
  [DistributorType] = @DistributorType,
  [CounterLocation] = @CounterLocation
  Where Id = @UserId  
  declare @FirstName nvarchar(100)
  declare @MiddleName nvarchar(100)
  declare @LastName nvarchar(100)
  declare @Password varchar(400)
  select @FirstName = FirstName, @LastName = LastName, @MiddleName = MiddleName, @Password = Password from [RetailUser] Where Id = @UserId
  SELECT @OperationMessage = 'Successfully updated user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';    
    
 END    
    
 SELECT @OperationMessage AS OperationMessage;     
                   
 COMMIT
 
 Go
 
 Alter Table [dbo].[RetailUser]
Add AgreementAccepted tinyint

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
 declare @ActivatedTill datetime    
 declare @days int    
 SELECT @ActivatedTill = ActivatedTill FROm RetailUser WITh(NOLOCK) WHERE OrderNo=@OutResult;     
 if(@ActivatedTill is not null)    
 begin    
  if(@ActivatedTill < GetDate())    
  begin    
   UPDATE RetailUser SET Active=0,ActivatedTill = null  WHERE OrderNo=@OutResult;     
  end    
 end    
     
 SELECT Id, UserType, MarginType, ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName,'') + ' ' + ISNULL(LastName,'') AS RetailerName, Mobile AS MobileNumber,         
  City, EMail, MasterId as Parent, [Address], OrderNo AS USL, Active, DefaultUtilityOperator, ActivatedTill, AgreementAccepted       
  FROM RetailUser WITH(NOLOCK) WHERE OrderNo=@OutResult;        
      
  if((Select Count(1) from [dbo].[UserLoginTimeInfo] where CAST(LoginTime as date) = CAST(getdate() as date)) = 0)      
  begin      
      insert into [dbo].[UserLoginTimeInfo](RetailUserId, LoginTime) values(@Id, getdate())      
  end   
    if(@OutResult is null)    
	 begin
	    SELECT @OutResult = ISNULL(OrderNo,0), @Id = Id from [RetailUser] with(nolock) WHERE [Mobile] = @MobileNumber AND [Password]=@Password;
		IF(@OutResult > 0)        
        BEGIN 
			set @OutResult = -1
		end
	 end
 COMMIT
 
 Go
 
 Create Proc usp_UpdateAgreementStatus
 @Id varchar(10)
 As 
 Update RetailUser set AgreementAccepted = 1 where Id = @Id