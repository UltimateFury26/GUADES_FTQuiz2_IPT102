/* FTQuiz2 Phase 02 verification - run AFTER GUADES_MoguMogu_BusinessUpgrade_IPT102.sql.
   Run the upgrade script a second time, then re-run this: every count must stay the same. */
USE MoguMogu_DB;
GO
/* 1. Object counts (old portfolio objects are kept: 9 + 4 tables, 49 + 17 procedures) */
SELECT DB_NAME() AS CurrentDb,
       (SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo'))        AS Tables_Expect13,
       (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'usp[_]%')             AS Procs_Expect66,
       (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'usp[_]BusinessProfile[_]%'
                                              OR name LIKE 'usp[_]ContactInfo[_]%'
                                              OR name LIKE 'usp[_]Owner[_]%'
                                              OR name LIKE 'usp[_]Product[_]%')    AS NewProcs_Expect17;

/* 2. Row counts */
SELECT 'Users' T, COUNT(*) N FROM dbo.Users
UNION ALL SELECT 'BusinessProfile (expect 1)', COUNT(*) FROM dbo.BusinessProfile
UNION ALL SELECT 'ContactInfo (expect 1)',     COUNT(*) FROM dbo.ContactInfo
UNION ALL SELECT 'Owners (expect >= 1)',       COUNT(*) FROM dbo.Owners
UNION ALL SELECT 'Products (expect >= 4)',     COUNT(*) FROM dbo.Products;

/* 3. Singleton guard: a second BusinessProfile row must be rejected (expect error 547) */
BEGIN TRY
    INSERT INTO dbo.BusinessProfile (BusinessProfileId, BusinessName) VALUES (2, N'x');
    SELECT 'FAIL: singleton check missing' AS Result;
END TRY
BEGIN CATCH
    SELECT 'PASS: singleton enforced' AS Result, ERROR_NUMBER() AS ErrorNumber;
END CATCH;

/* 4. Procedure behaviour, rolled back so no data is left behind */
BEGIN TRAN;
    DECLARE @new TABLE (NewId INT);
    INSERT INTO @new EXEC dbo.usp_Product_Insert @ProductName = N'ZZ Test Product', @Flavor = N'Test', @IsPublished = 0;
    DECLARE @id INT = (SELECT TOP 1 NewId FROM @new);

    SELECT 'Insert: unpublished product exists in GetAll'       AS Step, COUNT(*) AS Expect1 FROM dbo.Products WHERE ProductId = @id;
    EXEC dbo.usp_Product_GetPublished;                                           -- must NOT list ZZ Test Product
    EXEC dbo.usp_Product_SetPublished @ProductId = @id, @IsPublished = 1;        -- Affected = 1
    SELECT 'Published product now listed'                       AS Step, COUNT(*) AS Expect1 FROM dbo.Products WHERE ProductId = @id AND IsPublished = 1;
    EXEC dbo.usp_Product_Update @ProductId = @id, @ProductName = N'ZZ Test 2', @ImagePath = N'FTQuiz2_Media/Products/a.png';
    EXEC dbo.usp_Product_Update @ProductId = @id, @ProductName = N'ZZ Test 3', @ImagePath = NULL;   -- NULL keeps image
    SELECT 'Image kept when NULL passed'                        AS Step, ImagePath AS Expect_a_png FROM dbo.Products WHERE ProductId = @id;
    EXEC dbo.usp_Product_Delete @ProductId = @id;                                -- Affected = 1
ROLLBACK TRAN;
GO
