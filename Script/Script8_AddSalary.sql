USE [ESuvidha]
GO

--==========================================

Alter Table RetailUser Add Wages int

--=========================================
GO
/****** Object:  StoredProcedure [dbo].[usp_updateSalaryAmount]    Script Date: 17-12-2024 15:17:24 ******/
DROP PROCEDURE [dbo].[usp_updateSalaryAmount]
GO

/****** Object:  StoredProcedure [dbo].[usp_updateSalaryAmount]    Script Date: 17-12-2024 15:17:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[usp_updateSalaryAmount]   
    @RetailUserId varchar(14),  
 @SalaryAmount int

AS   
 Declare @OperationMessage nvarchar(500)  
 DECLARE @RetailUserOrderNo INT  
 
   
 UPDATE RetailUser   
    SET Wages= @SalaryAmount
     WHERE Id=@RetailUserId;  

 SELECT @OperationMessage =  'Success: Salary updated successfully.'  
 SELECT @OperationMessage as OperationMessage;

GO

--=====================
/****** Object:  StoredProcedure [dbo].[usp_RetailUserListWithUPPCLCommission]    Script Date: 17-12-2024 15:21:56 ******/
DROP PROCEDURE [dbo].[usp_RetailUserListWithUPPCLCommission]
GO

/****** Object:  StoredProcedure [dbo].[usp_RetailUserListWithUPPCLCommission]    Script Date: 17-12-2024 15:21:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[usp_RetailUserListWithUPPCLCommission]
@dateFrom datetime,
@dateTo datetime,
@Id varchar(10)
As
Begin

	 Select RetailUserId,
	 Sum(Case when RTRim(LTRIM(UPPCL_PaymentType)) = 'FULL' and Amount < cast(2000 as decimal) then 20 else amount * .5 / 100 end) UPPCL_Commission 
	 into #temp 
	 from RTran 
	 Where cast(CreateDate as date) between cast(@dateFrom as date) and cast(@dateTo as date) and RTRim(LTRIM(UPPCL_PaymentType)) in ('FULL', 'PARTIAL')
	 Group By RetailUserId

	 declare @Days int
	 set @Days = DATEDIFF(DAY,@dateFrom,@dateTo) + 1
	 

	SELECT RU.Id, OrderNo AS USL, Case When UserType = 5 then 'Retailer' when UserType = 6 then 'Distributor' else 'Master Distributor' end UserType, 
	[dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName,
	ISNULL(FirstName,'')+' '+ISNULL(MiddleName,'')+' ' + ISNULL(LastName,'') As RName,
	Mobile AS MobileNumber,      
	MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName,
	City, EMail, [Address], Active,
	UPPCL_Commission, ( (IsNULL(Wages,0)/30) * @Days) as Salary
	FROM RetailUser RU WITH(NOLOCK) Left join #temp t on t.RetailUserId = RU.Id  
	where UserType = 5 and 1 = case when ((@Id is not null and OrderNo = @Id) or @Id is null) then 1 else 0 end 
	ORDER BY OrderNo ASC 
End

GO