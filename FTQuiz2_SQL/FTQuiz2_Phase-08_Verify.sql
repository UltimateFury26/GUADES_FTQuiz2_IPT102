/* FTQuiz2 Phase 08 - FINAL verification (read-only). Run in SSMS after the setup/upgrade scripts.
   Every block should show the expected result written beside it. */
USE MoguMogu_DB;
GO

/* 1. Required tables. Expected: 7 rows, all Present. */
SELECT t.TableName,
       CASE WHEN OBJECT_ID(N'dbo.' + t.TableName, N'U') IS NULL THEN 'MISSING' ELSE 'Present' END AS Status
FROM (VALUES (N'Users'), (N'Posts'), (N'PageVisits'), (N'BusinessProfile'),
             (N'ContactInfo'), (N'Owners'), (N'Products')) AS t(TableName);
GO

/* 2. Every stored procedure the application calls (taken from the C# repositories).
      Expected: 40 rows, all Present. */
SELECT p.ProcName,
       CASE WHEN OBJECT_ID(N'dbo.' + p.ProcName, N'P') IS NULL THEN 'MISSING' ELSE 'Present' END AS Status
FROM (VALUES
 (N'usp_User_Register'), (N'usp_User_Login'), (N'usp_User_GetById'), (N'usp_User_UpdateAccount'), (N'usp_User_ChangePassword'),
 (N'usp_Admin_GetUsers'), (N'usp_Admin_SetUserStatus'), (N'usp_Admin_DeleteUser'),
 (N'usp_BusinessProfile_Get'), (N'usp_BusinessProfile_Update'), (N'usp_ContactInfo_Get'), (N'usp_ContactInfo_Update'),
 (N'usp_Owner_GetAll'), (N'usp_Owner_GetById'), (N'usp_Owner_Insert'), (N'usp_Owner_Update'), (N'usp_Owner_Delete'),
 (N'usp_Product_GetAll'), (N'usp_Product_GetPublished'), (N'usp_Product_GetById'), (N'usp_Product_Insert'),
 (N'usp_Product_Update'), (N'usp_Product_SetPublished'), (N'usp_Product_SetSortOrder'), (N'usp_Product_Delete'),
 (N'usp_Post_GetAll'), (N'usp_Post_GetPublished'), (N'usp_Post_GetById'), (N'usp_Post_Insert'),
 (N'usp_Post_Update'), (N'usp_Post_SetPublished'), (N'usp_Post_Delete'),
 (N'usp_Report_UserSummary'), (N'usp_Report_SignupsByMonth'), (N'usp_Report_RecentUsers'), (N'usp_Report_ContentSummary'),
 (N'usp_Report_ProductSummary'), (N'usp_Report_VisitSummary'), (N'usp_Report_VisitsByMonth'), (N'usp_Visit_Record')
) AS p(ProcName)
ORDER BY CASE WHEN OBJECT_ID(N'dbo.' + p.ProcName, N'P') IS NULL THEN 0 ELSE 1 END, p.ProcName;
GO

/* 3. Accounts. Expected: at least one Active Admin; no password data is shown here. */
SELECT [Role], [Status], COUNT(*) AS Accounts FROM dbo.Users GROUP BY [Role], [Status] ORDER BY [Role], [Status];
GO

/* 4. Website content the four required pages read. Expected: BusinessProfile = 1, ContactInfo = 1, Owners >= 1, Products >= 1. */
SELECT 'BusinessProfile' AS Item, COUNT(*) AS N FROM dbo.BusinessProfile
UNION ALL SELECT 'ContactInfo', COUNT(*) FROM dbo.ContactInfo
UNION ALL SELECT 'Owners', COUNT(*) FROM dbo.Owners
UNION ALL SELECT 'Products (published)',   COUNT(*) FROM dbo.Products WHERE IsPublished = 1
UNION ALL SELECT 'Products (unpublished)', COUNT(*) FROM dbo.Products WHERE IsPublished = 0;
GO

/* 5. Content still waiting for REAL, verified information before the demo.
      Rows here are not errors - they are the placeholders the administrator must replace. */
SELECT 'BusinessProfile.BrandStory' AS Field, LEFT(BrandStory, 80) AS Value FROM dbo.BusinessProfile WHERE BrandStory LIKE N'%[[]Placeholder%'
UNION ALL SELECT 'Owners.FullName', FullName FROM dbo.Owners WHERE FullName LIKE N'%to be completed%' OR Bio LIKE N'%[[]Placeholder%'
UNION ALL SELECT 'ContactInfo.Email (empty)', N'(empty)' FROM dbo.ContactInfo WHERE Email IS NULL
UNION ALL SELECT 'ContactInfo.Phone (empty)', N'(empty)' FROM dbo.ContactInfo WHERE Phone IS NULL
UNION ALL SELECT 'ContactInfo.Address (empty)', N'(empty)' FROM dbo.ContactInfo WHERE [Address] IS NULL;
GO

/* 6. Image paths. They must be local project paths. Expected: no rows (anything listed is a remote URL or an
      unexpected folder). Whether the file exists on disk is checked in the browser (no broken images). */
SELECT 'Products' AS Source, ProductId AS Id, ImagePath AS Path FROM dbo.Products
WHERE  ImagePath IS NOT NULL AND ImagePath NOT LIKE N'FTQuiz2[_]Uploads/%' AND ImagePath NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'BusinessProfile.Hero', 1, HeroImagePath FROM dbo.BusinessProfile
WHERE  HeroImagePath IS NOT NULL AND HeroImagePath NOT LIKE N'FTQuiz2[_]Uploads/%' AND HeroImagePath NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'BusinessProfile.About', 1, AboutImagePath FROM dbo.BusinessProfile
WHERE  AboutImagePath IS NOT NULL AND AboutImagePath NOT LIKE N'FTQuiz2[_]Uploads/%' AND AboutImagePath NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'Owners', OwnerId, PhotoPath FROM dbo.Owners
WHERE  PhotoPath IS NOT NULL AND PhotoPath NOT LIKE N'FTQuiz2[_]Uploads/%' AND PhotoPath NOT LIKE N'FTQuiz2[_]Media/%';
GO

/* 7. Reports must describe the business. Expected: sections for profile, contact, owners, products, announcements only. */
EXEC dbo.usp_Report_ContentSummary;
GO

/* 8. Old portfolio tables. Informational: 'Present' is fine (nothing in the application reads them any more).
      Run GUADES_MoguMogu_RetirePortfolio_IPT102.sql only if you want them removed. */
SELECT t.TableName,
       CASE WHEN OBJECT_ID(N'dbo.' + t.TableName, N'U') IS NULL THEN 'Removed' ELSE 'Present (unused)' END AS Status
FROM (VALUES (N'PersonalInfo'), (N'Education'), (N'Skills'), (N'Hobbies'), (N'Projects'), (N'GalleryItems')) AS t(TableName);
GO
