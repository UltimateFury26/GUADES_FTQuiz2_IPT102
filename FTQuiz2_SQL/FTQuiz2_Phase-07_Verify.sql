/* FTQuiz2 Phase 07 verification - read-only. Compare each pair of rows with what the Reports page shows. */
USE MoguMogu_DB;
GO

/* 1. Users: procedure vs direct count (the two rows must match) */
EXEC dbo.usp_Report_UserSummary;
SELECT COUNT(*) AS TotalUsers,
       SUM(CASE WHEN [Status]='Active'   THEN 1 ELSE 0 END) AS ActiveUsers,
       SUM(CASE WHEN [Status]='Inactive' THEN 1 ELSE 0 END) AS InactiveUsers,
       SUM(CASE WHEN [Role]='Admin'      THEN 1 ELSE 0 END) AS AdminUsers
FROM dbo.Users;
GO

/* 2. Products: procedure vs direct count */
EXEC dbo.usp_Report_ProductSummary;
SELECT COUNT(*) AS TotalProducts,
       SUM(CASE WHEN IsPublished=1 THEN 1 ELSE 0 END) AS PublishedProducts,
       SUM(CASE WHEN IsPublished=0 THEN 1 ELSE 0 END) AS UnpublishedProducts
FROM dbo.Products;
GO

/* 3. Visits (signed-in Users, Home page only): procedure vs direct count */
EXEC dbo.usp_Report_VisitSummary;
SELECT COUNT(*) AS TotalVisits, COUNT(DISTINCT UserId) AS DistinctUsers FROM dbo.PageVisits;
GO

/* 4. Business content summary: must list business sections only (no Education/Skills/Hobbies/Projects/Gallery) */
EXEC dbo.usp_Report_ContentSummary;
GO

/* 5. 12-month charts: 12 rows each, oldest first, zero months included */
EXEC dbo.usp_Report_SignupsByMonth;
EXEC dbo.usp_Report_VisitsByMonth;
GO

/* 6. Visits recorded for an Admin must be 0 (rows here = a problem) */
SELECT v.VisitId, v.UserId FROM dbo.PageVisits v JOIN dbo.Users u ON u.UserId = v.UserId WHERE u.[Role] = 'Admin';
GO
