USE [ESuvidha]
GO
/****** Object:  StoredProcedure [dbo].[usp_updateUserActivationState]    Script Date: 28-11-2024 11:41:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 ALTER PROC [dbo].[usp_updateUserActivationState] 
    @RetailUserId varchar(14),
	@updateChild int
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

	-- udpate the child retailers if updateChild param is 1.
	if(@updateChild = 1)
	begin
		UPDATE RetailUser SET [Active]=@Active WHERE Id in (Select Id from RetailUser where MasterId = @RetailUserId)
	end

	
	SELECT @OperationMessage as OperationMessage;
	
