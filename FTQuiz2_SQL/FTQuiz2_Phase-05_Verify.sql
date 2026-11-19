/* FTQuiz2 Phase 05 verification - run in SSMS against the project database (read-only except block 5,
   which is wrapped in a transaction and rolled back). Use it after testing the admin pages. */
USE MoguMogu_DB;
GO

/* 1. What the admin pages manage */
SELECT 'BusinessProfile'                AS Item, COUNT(*) AS N FROM dbo.BusinessProfile
UNION ALL SELECT 'ContactInfo',                   COUNT(*) FROM dbo.ContactInfo
UNION ALL SELECT 'Owners',                        COUNT(*) FROM dbo.Owners
UNION ALL SELECT 'Products - published',          COUNT(*) FROM dbo.Products WHERE IsPublished = 1
UNION ALL SELECT 'Products - unpublished',        COUNT(*) FROM dbo.Products WHERE IsPublished = 0;
GO

/* 2. Product display order (the admin list and the public pages use this exact order).
      After using the up/down arrows the SortOrder values are 10, 20, 30 ... with no duplicates. */
SELECT ProductId, ProductName, Flavor, IsPublished, SortOrder, ImagePath
FROM   dbo.Products
ORDER BY SortOrder, ProductId;

SELECT SortOrder, COUNT(*) AS HowMany
FROM   dbo.Products
GROUP BY SortOrder
HAVING COUNT(*) > 1;                        -- rows here = products sharing a position (fixed by the first arrow click)
GO

/* 3. Every stored image path should live in FTQuiz2_Uploads (admin uploads) or FTQuiz2_Media (seeded files).
      Rows returned here are unexpected values that the admin pages would never write. */
SELECT 'BusinessProfile.HeroImagePath'  AS Source, HeroImagePath  AS Path FROM dbo.BusinessProfile
WHERE  HeroImagePath  IS NOT NULL AND HeroImagePath  NOT LIKE N'FTQuiz2[_]Uploads/%' AND HeroImagePath  NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'BusinessProfile.AboutImagePath', AboutImagePath FROM dbo.BusinessProfile
WHERE  AboutImagePath IS NOT NULL AND AboutImagePath NOT LIKE N'FTQuiz2[_]Uploads/%' AND AboutImagePath NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'Owners.PhotoPath', PhotoPath FROM dbo.Owners
WHERE  PhotoPath IS NOT NULL AND PhotoPath NOT LIKE N'FTQuiz2[_]Uploads/%' AND PhotoPath NOT LIKE N'FTQuiz2[_]Media/%'
UNION ALL
SELECT 'Products.ImagePath', ImagePath FROM dbo.Products
WHERE  ImagePath IS NOT NULL AND ImagePath NOT LIKE N'FTQuiz2[_]Uploads/%' AND ImagePath NOT LIKE N'FTQuiz2[_]Media/%';
GO

/* 4. Text still marked as a placeholder (must be completed before the site is presented) */
SELECT 'Brand story' AS Item, LEFT(BrandStory, 80) AS Value FROM dbo.BusinessProfile WHERE BrandStory LIKE N'[[]%]'
UNION ALL SELECT 'Owner', FullName FROM dbo.Owners WHERE FullName LIKE N'[[]%]' OR Bio LIKE N'[[]%]';
GO

/* 5. "No new image keeps the current image" (the procedure contract the admin pages rely on).
      Runs inside a transaction and is rolled back, so nothing changes. */
BEGIN TRAN;
    DECLARE @id INT = (SELECT TOP (1) ProductId FROM dbo.Products WHERE ImagePath IS NOT NULL ORDER BY ProductId);
    IF @id IS NOT NULL
    BEGIN
        DECLARE @before NVARCHAR(260) = (SELECT ImagePath FROM dbo.Products WHERE ProductId = @id);

        EXEC dbo.usp_Product_Update @ProductId = @id, @ProductName = N'Verify test', @ImagePath = NULL;   -- NULL image path

        SELECT @id AS ProductId, @before AS ImageBefore,
               (SELECT ImagePath FROM dbo.Products WHERE ProductId = @id) AS ImageAfter;                 -- the two must be equal
    END
ROLLBACK TRAN;
GO
