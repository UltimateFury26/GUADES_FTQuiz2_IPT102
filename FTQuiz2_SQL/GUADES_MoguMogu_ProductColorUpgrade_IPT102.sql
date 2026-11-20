/* =====================================================================
   MoguMogu_DB - PRODUCT BACKGROUND COLOR UPGRADE
   Run AFTER GUADES_MoguMogu_BusinessUpgrade_IPT102.sql. Re-runnable.

   Adds dbo.Products.BackgroundColor (a hex colour such as #FDE3A3) so the Admin can
   match the Products page carousel background to the colour of each drink.
     - NULL means "automatic": the site falls back to its default pastel rotation.
     - The five usp_Product_* procedures that read or write a product are replaced.
       usp_Product_Update: @BackgroundColor NULL = keep current, N'' = clear (automatic).
   ===================================================================== */
USE MoguMogu_DB;
GO

IF COL_LENGTH(N'dbo.Products', N'BackgroundColor') IS NULL
    ALTER TABLE dbo.Products ADD BackgroundColor NVARCHAR(7) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_Products_BackgroundColor' AND parent_object_id = OBJECT_ID(N'dbo.Products'))
    ALTER TABLE dbo.Products ADD CONSTRAINT CK_Products_BackgroundColor
        CHECK (BackgroundColor IS NULL
               OR BackgroundColor LIKE N'#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]');
GO

/* ---------- Readers: same rows as before plus BackgroundColor ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Product_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductId, ProductName, Flavor, ShortDescription, FullDescription, ImagePath,
           SortOrder, IsPublished, CreatedAt, UpdatedAt, BackgroundColor
    FROM   dbo.Products
    ORDER BY SortOrder, ProductId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetPublished
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductId, ProductName, Flavor, ShortDescription, FullDescription, ImagePath,
           SortOrder, IsPublished, CreatedAt, UpdatedAt, BackgroundColor
    FROM   dbo.Products
    WHERE  IsPublished = 1
    ORDER BY SortOrder, ProductId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetById
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductId, ProductName, Flavor, ShortDescription, FullDescription, ImagePath,
           SortOrder, IsPublished, CreatedAt, UpdatedAt, BackgroundColor
    FROM   dbo.Products
    WHERE  ProductId = @ProductId;
END
GO

/* ---------- Writers ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Product_Insert
    @ProductName      NVARCHAR(100),
    @Flavor           NVARCHAR(50)  = NULL,
    @ShortDescription NVARCHAR(250) = NULL,
    @FullDescription  NVARCHAR(MAX) = NULL,
    @ImagePath        NVARCHAR(260) = NULL,
    @SortOrder        INT           = NULL,
    @IsPublished      BIT           = 1,
    @BackgroundColor  NVARCHAR(7)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@ProductName, N'')))) = 0
        THROW 50001, 'Product name is required.', 1;

    INSERT INTO dbo.Products (ProductName, Flavor, ShortDescription, FullDescription, ImagePath, SortOrder, IsPublished, BackgroundColor)
    VALUES (LTRIM(RTRIM(@ProductName)), NULLIF(LTRIM(RTRIM(@Flavor)), N''), @ShortDescription, @FullDescription, @ImagePath,
            ISNULL(@SortOrder, (SELECT ISNULL(MAX(SortOrder), 0) + 10 FROM dbo.Products)),
            ISNULL(@IsPublished, 1),
            NULLIF(LTRIM(RTRIM(@BackgroundColor)), N''));

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_Update
    @ProductId        INT,
    @ProductName      NVARCHAR(100),
    @Flavor           NVARCHAR(50)  = NULL,
    @ShortDescription NVARCHAR(250) = NULL,
    @FullDescription  NVARCHAR(MAX) = NULL,
    @ImagePath        NVARCHAR(260) = NULL,   -- NULL = keep current image
    @SortOrder        INT           = NULL,   -- NULL = keep current order
    @IsPublished      BIT           = NULL,   -- NULL = keep current status
    @BackgroundColor  NVARCHAR(7)   = NULL    -- NULL = keep current colour, N'' = clear (automatic)
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@ProductName, N'')))) = 0
        THROW 50001, 'Product name is required.', 1;

    UPDATE dbo.Products
    SET    ProductName      = LTRIM(RTRIM(@ProductName)),
           Flavor           = NULLIF(LTRIM(RTRIM(@Flavor)), N''),
           ShortDescription = @ShortDescription,
           FullDescription  = @FullDescription,
           ImagePath        = ISNULL(@ImagePath, ImagePath),
           SortOrder        = ISNULL(@SortOrder, SortOrder),
           IsPublished      = ISNULL(@IsPublished, IsPublished),
           BackgroundColor  = CASE WHEN @BackgroundColor IS NULL THEN BackgroundColor
                                   ELSE NULLIF(LTRIM(RTRIM(@BackgroundColor)), N'') END,
           UpdatedAt        = SYSDATETIME()
    WHERE  ProductId = @ProductId;

    SELECT @@ROWCOUNT AS Affected;
END
GO

/* ---------- Starting colours for the four seeded flavours (only where none is set yet) ---------- */
UPDATE dbo.Products SET BackgroundColor = N'#F8D3E1' WHERE BackgroundColor IS NULL AND ProductName = N'Mogu Mogu Lychee';
UPDATE dbo.Products SET BackgroundColor = N'#FDE3A3' WHERE BackgroundColor IS NULL AND ProductName = N'Mogu Mogu Mango';
UPDATE dbo.Products SET BackgroundColor = N'#DCEBB8' WHERE BackgroundColor IS NULL AND ProductName = N'Mogu Mogu Melon';
UPDATE dbo.Products SET BackgroundColor = N'#F9CDBD' WHERE BackgroundColor IS NULL AND ProductName = N'Mogu Mogu Peach';
GO

SELECT ProductId, ProductName, BackgroundColor FROM dbo.Products ORDER BY SortOrder, ProductId;
GO
