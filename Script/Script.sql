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
	
    
