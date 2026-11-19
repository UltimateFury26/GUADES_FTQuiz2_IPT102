/* =====================================================================
   MoguMogu_DB - PHASE 7: BUSINESS REPORTS UPGRADE
   Run AFTER GUADES_QuizScript_IPT102.sql and GUADES_MoguMogu_BusinessUpgrade_IPT102.sql.
   Re-runnable (CREATE OR ALTER only). Creates/changes procedures; no table is touched.
     - usp_Report_ProductSummary  NEW      total / published / unpublished products
     - usp_Report_ContentSummary  REPLACED business content records instead of portfolio tables
   ===================================================================== */
USE MoguMogu_DB;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_ProductSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)                                                       AS TotalProducts,
           ISNULL(SUM(CASE WHEN IsPublished = 1 THEN 1 ELSE 0 END), 0)    AS PublishedProducts,
           ISNULL(SUM(CASE WHEN IsPublished = 0 THEN 1 ELSE 0 END), 0)    AS UnpublishedProducts
    FROM   dbo.Products;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_ContentSummary
AS
BEGIN
    SET NOCOUNT ON;

    SELECT N'Business profile (About)'     AS Section, COUNT(*) AS Items FROM dbo.BusinessProfile
    UNION ALL SELECT N'Contact information',            COUNT(*) FROM dbo.ContactInfo
    UNION ALL SELECT N'Owners / company information',   COUNT(*) FROM dbo.Owners
    UNION ALL SELECT N'Products (published)',           COUNT(*) FROM dbo.Products WHERE IsPublished = 1
    UNION ALL SELECT N'Products (unpublished)',         COUNT(*) FROM dbo.Products WHERE IsPublished = 0
    UNION ALL SELECT N'Announcements (published)',      COUNT(*) FROM dbo.Posts    WHERE IsPublished = 1
    UNION ALL SELECT N'Announcements (draft)',          COUNT(*) FROM dbo.Posts    WHERE IsPublished = 0;
END
GO

SELECT name AS ProcedureName FROM sys.procedures
WHERE  name IN ('usp_Report_ProductSummary', 'usp_Report_ContentSummary') ORDER BY name;
GO
