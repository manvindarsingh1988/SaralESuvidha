

CREATE PROC [dbo].[usp_RetailUserListWithBalance] 
AS 

SELECT Id, [dbo].[UserTypeString](UserType) AS UserType, MarginType, [dbo].[RetailerNamewithIdMobile](Id) AS RetailerName, [dbo].[RetailerName](Id) AS RName, Mobile AS MobileNumber, ISNULL([dbo].[RetailUserBalance](Id),0) AS Balance, ISNULL([dbo].[RetailClientOrderNoUserNameMobile](MasterId),'') AS ParentName, 
City, EMail, MasterId as Parent, [Address], OrderNo AS USL, Active
FROM RetailUser WITH(NOLOCK) ORDER BY OrderNo ASC


GO


