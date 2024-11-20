CREATE PROC [dbo].[usp_GetDistributorList] 
@UserType int
AS 
SELECT Id, ([dbo].[RetailerNamewithIdMobile](Id) + ' (' + Id + ')') AS DistributorName FROM RetailUser WITH(NOLOCK) where UserType = @UserType

Go

CREATE PROC [dbo].[usp_updateUserActivationState] 
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
	UPDATE RetailUser SET [Active]=@Active WHERE Id=@RetailUserId;
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

Create PROC [dbo].[usp_updateDistributor] 
    @RetailUserId varchar(14),
	@DistributorId varchar(14)
AS 
	DECLARE @MasterId nvarchar(14) 
	Declare @OperationMessage nvarchar(500)
	DECLARE @RetailUserOrderNo INT
	SELECT @MasterId = MasterId, @RetailUserOrderNo = OrderNo from RetailUser WITH(NOLOCK) WHERE Id=@RetailUserId;
	if(@MasterId = @DistributorId)
	begin 
	   SELECT @OperationMessage =  'Errors: user ' + CONVERT(VARCHAR(20),@RetailUserOrderNo) + ' is already linked with same distributor.'
    end
	else
	begin
	   if((Select count(1) from RetailUser where Id = @DistributorId) = 1)
	   begin
			UPDATE RetailUser SET MasterId=@DistributorId WHERE Id=@RetailUserId;
			UPDATE UtilityMargin SET FixMargin = 0, MarginPercent = 0, MaxMargin = 0, MaxDailyLimit = 0 Where RetailUserid = @RetailUserId
			SELECT @OperationMessage =  'Success: user (' + CONVERT(VARCHAR(20),@RetailUserOrderNo) + ')''s distributor update successfully.'
	   end
	   else
	   begin
			SELECT @OperationMessage =  'Errors: Valid distributor is not selected'
	   end
    end
	
	SELECT @OperationMessage as OperationMessage;
	
Go

CREATE PROC [dbo].[usp_GetUserDetails] 
@RetailUserId varchar(50)
AS 
SELECT Id, UserType, MarginType, [dbo].[RetailerNamewithIdMobile](Id) AS RetailerName, Mobile AS MobileNumber,  
City, EMail, MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, [Address], OrderNo AS USL, Active
FROM RetailUser WITH(NOLOCK) where Id = @RetailUserId or Mobile = @RetailUserId or Cast(OrderNo as nvarchar(50)) = @RetailUserId

GO

CREATE TABLE [dbo].[UserLoginTimeInfo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RetailUserId] [varchar](10) NULL,
	[LoginTime] [datetime] NULL,
 CONSTRAINT [PK_UserLoginTimeInfo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserLoginTimeInfo]  WITH CHECK ADD  CONSTRAINT [FK_UserLoginTimeInfo_RetailUserId] FOREIGN KEY([RetailUserId])
REFERENCES [dbo].[RetailUser] ([Id])
GO

ALTER TABLE [dbo].[UserLoginTimeInfo] CHECK CONSTRAINT [FK_UserLoginTimeInfo_RetailUserId]
GO	

Alter PROC [dbo].[usp_RetailUserList] 
AS 
SELECT RU.Id, UserType, MarginType, [dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName, Mobile AS MobileNumber,  
City, EMail, MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, [Address], OrderNo AS USL, Active, LoginTime
FROM RetailUser RU WITH(NOLOCK)
Left Join UserLoginTimeInfo ULTI on ULTI.RetailUserId = RU.ID and CAST(LoginTime as date) = CAST(getdate() as date)
ORDER BY OrderNo ASC

GO

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
  
 SELECT Id, UserType, MarginType, ISNULL(FirstName,'') + ' ' + ISNULL(MiddleName,'') + ' ' + ISNULL(LastName,'') AS RetailerName, Mobile AS MobileNumber,   
  City, EMail, MasterId as Parent, [Address], OrderNo AS USL, Active, DefaultUtilityOperator  
  FROM RetailUser WITH(NOLOCK) WHERE OrderNo=@OutResult;  

  if((Select Count(1) from [dbo].[UserLoginTimeInfo] where CAST(LoginTime as date) = CAST(getdate() as date)) = 0)
  begin
      insert into [dbo].[UserLoginTimeInfo](RetailUserId, LoginTime) values(@Id, getdate())
  end
 COMMIT  

	
    
