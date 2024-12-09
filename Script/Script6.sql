create procedure usp_RetailUserListWithBalanceByUserAndDate
@date datetime,
@Id varchar(10)
As
Begin
	SELECT RU.Id, OrderNo AS USL, Case When UserType = 5 then 'Retailer' when UserType = 6 then 'Distributor' else 'Master Distributor' end UserType, 
	[dbo].[RetailerNamewithIdMobile](RU.Id) AS RetailerName,
	ISNULL(FirstName,'')+' '+ISNULL(MiddleName,'')+' ' + ISNULL(LastName,'') As RName,
	Mobile AS MobileNumber,      
	MasterId as Parent, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName,
	City, EMail, [Address], Active,
	(Select Top 1 ClosingBalance from RTran where RetailUserId = RU.Id and cast(CreateDate as date) = cast(@date as date) order by CreateDate desc) Balance
	FROM RetailUser RU WITH(NOLOCK)  
	where 1 = case when ((@Id is not null and OrderNo = @Id) or @Id is null) then 1 else 0 end 
	ORDER BY OrderNo ASC 
End