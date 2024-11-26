USE [ESuvidha]
GO

IF COL_LENGTH('RetailUser','DistributorType') IS NULL
Begin
	Alter Table [RetailUser] Add DistributorType varchar(50)
End


/****** Object:  StoredProcedure [dbo].[usp_RetailUserInsert]    Script Date: 26-11-2024 14:30:07 ******/
if(OBJECT_ID('usp_RetailUserInsert') IS NOT NULL)
Begin
	DROP PROCEDURE [dbo].[usp_RetailUserInsert]
End
GO

/****** Object:  StoredProcedure [dbo].[usp_RetailUserInsert]    Script Date: 26-11-2024 14:30:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[usp_RetailUserInsert]  
	@Id varchar(10) = NULL,
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
	@DistributorType varchar(50) = NULL

AS   
 SET NOCOUNT ON   
 SET XACT_ABORT ON    
   
 BEGIN TRAN  
   
 DECLARE @UserCount smallint = 0 DECLARE @EmailCount smallint = 0 DECLARE @OperationMessage varchar(250);  
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

 If @Id IS NOT NULL 
 Begin
  Update [dbo].[RetailUser]
  Set [MasterId] = @MasterId, [UserType] = @UserType, [MarginType] = @MarginType, [FirstName] = @FirstName, [MiddleName] = @MiddleName, [LastName] = @LastName,
	  [EMail] = @Email, [Gender] = @Gender, [DateOfBirth] = @DateOfBirth, [Mobile] = @Mobile, [Address] = @Address, [City] = @City, [PinCode] = @PinCode, 
	  [Country] = @Country, [StateName] = @StateName, [Password] = @Password, [CreditLimit] = @CreditLimit, [MinFundValue] = @MinFundValue,
	  [Commission] = @Commission, [Active] = @Active,  [SignupDate] = @SignupDate, KYCRequired = 1, DistributorType = @DistributorType
  Where Id = @Id
    
	  SELECT @OperationMessage = 'Successfully updated user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';  
 End
 Else 
 Begin
	  SELECT @Id = RIGHT('RU00' + convert(nvarchar(10), next value for RetailUserNextId),10);  
  
	  INSERT INTO [dbo].[RetailUser] ([Id], [MasterId], [UserType], [MarginType], [FirstName], [MiddleName], [LastName], [EMail], [Gender], [DateOfBirth],   
	  [Mobile], [Address], [City], [PinCode], [Country], [StateName], [Password], [CreditLimit],   
	  [MinFundValue], [Commission], [Active],  [SignupDate], KYCRequired, DistributorType)  
	  SELECT @Id, @MasterId, @UserType, @MarginType, @FirstName, @MiddleName, @LastName, @EMail, @Gender, @DateOfBirth, @Mobile, @Address,  
	  @City, @PinCode, @Country, @StateName, @Password, @CreditLimit, @MinFundValue, @Commission,   
	  0, CURRENT_TIMESTAMP, 1,@DistributorType;  
    
	  SELECT @OperationMessage = 'Successfully created user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';  
  End

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

GO

--=======================================

USE [ESuvidha]
GO

/****** Object:  StoredProcedure [dbo].[usp_RetailUserUpdate]    Script Date: 26-11-2024 17:57:14 ******/
if(OBJECT_ID('usp_RetailUserUpdate') IS NOT NULL)
Begin
	DROP PROCEDURE [dbo].[usp_RetailUserUpdate]
End
GO
/****** Object:  StoredProcedure [dbo].[usp_RetailUserUpdate]    Script Date: 26-11-2024 17:57:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

  
CREATE PROC [dbo].[usp_RetailUserUpdate] 
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
	@PhysicalKYCDone int = null,
	@DistributorType varchar(50) = NULL
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
  [PhysicalKYCDone] = @PhysicalKYCDone,
  DistributorType = @DistributorType
  Where Id = @UserId
    
  SELECT @OperationMessage = 'Successfully updated user ' + @FirstName + ' ' + @MiddleName + ' ' + @LastName + ', Mobile(Login Id)-' + @Mobile + ' Login Password-' + @Password + '';  
  
 END  
  
 SELECT @OperationMessage AS OperationMessage;   
                 
 COMMIT

GO



