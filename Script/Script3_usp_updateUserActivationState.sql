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
	UPDATE RetailUser SET [Active]=@Active, KYCRequired = 0 WHERE Id=@RetailUserId;
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
	   UPDATE RetailUser SET [Active]= case when (KYCRequired = 1 or ActivatedTill is not null) then 0 else  @Active end,
	   ActivatedTill = null
	   WHERE MasterId= @RetailUserId
	   if (@UserType = 7)
	   begin
	    UPDATE RetailUser SET [Active]= case when (KYCRequired = 1 or ActivatedTill is not null) then 0 else  @Active end,
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