/* =====================================================================
   MoguMogu_DB - PHASE 2: BUSINESS CONTENT UPGRADE
   IPT102 - Mogu Mogu dynamic business website (ASP.NET MVC 5 + EF6 + MS SQL)

   Run AFTER GUADES_QuizScript_IPT102.sql (it needs dbo.Users and the existing
   portfolio tables to be present). This script is ADDITIVE and re-runnable:
     - it creates 4 new tables (BusinessProfile, ContactInfo, Owners, Products)
     - it creates 17 new stored procedures (CREATE OR ALTER)
     - it seeds only verified or clearly-marked placeholder data, guarded by IF NOT EXISTS
     - it does NOT drop or change Users, PageVisits, Posts or the old portfolio tables
       (the old portfolio tables are retired in the final cleanup phase)

   Business facts that are not verified are left NULL or marked
   "[Placeholder ...]" so the administrator can complete them from the Admin pages.
   ===================================================================== */

USE MoguMogu_DB;
GO

/* ---------------------------------------------------------------------
   1. BUSINESSPROFILE  (About the Business/Company - singleton row)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.BusinessProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusinessProfile
    (
        BusinessProfileId INT           NOT NULL CONSTRAINT DF_BusinessProfile_Id DEFAULT (1),
        BusinessName      NVARCHAR(100) NOT NULL,
        Tagline           NVARCHAR(200) NULL,
        BrandStory        NVARCHAR(MAX) NULL,
        Mission           NVARCHAR(500) NULL,
        Vision            NVARCHAR(500) NULL,
        AboutImagePath    NVARCHAR(260) NULL,
        HeroImagePath     NVARCHAR(260) NULL,
        UpdatedAt         DATETIME2(0)  NOT NULL CONSTRAINT DF_BusinessProfile_UpdatedAt DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_BusinessProfile           PRIMARY KEY (BusinessProfileId),
        CONSTRAINT CK_BusinessProfile_SingleRow CHECK (BusinessProfileId = 1)
    );
END
GO

/* ---------------------------------------------------------------------
   2. CONTACTINFO  (Contact Information - singleton row)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.ContactInfo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactInfo
    (
        ContactInfoId INT           NOT NULL CONSTRAINT DF_ContactInfo_Id DEFAULT (1),
        Email         NVARCHAR(100) NULL,
        Phone         VARCHAR(20)   NULL,
        [Address]     NVARCHAR(250) NULL,
        FacebookUrl   NVARCHAR(250) NULL,
        InstagramUrl  NVARCHAR(250) NULL,
        UpdatedAt     DATETIME2(0)  NOT NULL CONSTRAINT DF_ContactInfo_UpdatedAt DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_ContactInfo           PRIMARY KEY (ContactInfoId),
        CONSTRAINT CK_ContactInfo_SingleRow CHECK (ContactInfoId = 1)
    );
END
GO

/* ---------------------------------------------------------------------
   3. OWNERS  (Owners Information - one row per owner / company representative)
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Owners', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Owners
    (
        OwnerId   INT IDENTITY(1,1) NOT NULL,
        FullName  NVARCHAR(100)     NOT NULL,
        Position  NVARCHAR(100)     NULL,
        Bio       NVARCHAR(1000)    NULL,
        Email     NVARCHAR(100)     NULL,
        PhotoPath NVARCHAR(260)     NULL,
        SortOrder INT               NOT NULL CONSTRAINT DF_Owners_SortOrder DEFAULT (0),

        CONSTRAINT PK_Owners PRIMARY KEY (OwnerId)
    );
END
GO

/* ---------------------------------------------------------------------
   4. PRODUCTS  (Products and Services - the product catalogue)
   Size, price, ingredients and nutrition are intentionally NOT columns:
   they are only added if verified product data is supplied.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId        INT IDENTITY(1,1) NOT NULL,
        ProductName      NVARCHAR(100)     NOT NULL,
        Flavor           NVARCHAR(50)      NULL,
        ShortDescription NVARCHAR(250)     NULL,
        FullDescription  NVARCHAR(MAX)     NULL,
        ImagePath        NVARCHAR(260)     NULL,
        SortOrder        INT               NOT NULL CONSTRAINT DF_Products_SortOrder   DEFAULT (0),
        IsPublished      BIT               NOT NULL CONSTRAINT DF_Products_IsPublished DEFAULT (1),
        CreatedAt        DATETIME2(0)      NOT NULL CONSTRAINT DF_Products_CreatedAt   DEFAULT (SYSDATETIME()),
        UpdatedAt        DATETIME2(0)      NULL,

        CONSTRAINT PK_Products PRIMARY KEY (ProductId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Products_IsPublished_SortOrder' AND object_id = OBJECT_ID(N'dbo.Products'))
    CREATE INDEX IX_Products_IsPublished_SortOrder ON dbo.Products (IsPublished, SortOrder, ProductId);
GO

/* =====================================================================
   STORED PROCEDURES
   Conventions copied from the existing script: SET NOCOUNT ON; validation uses
   THROW 50001+; INSERT returns NewId; UPDATE/DELETE return Affected; a NULL image
   path on UPDATE means "keep the current image"; a NULL sort order means "last"
   on INSERT and "keep current" on UPDATE.
   ===================================================================== */

/* ---------- BusinessProfile ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_BusinessProfile_Get
AS
BEGIN
    SET NOCOUNT ON;

    SELECT BusinessProfileId, BusinessName, Tagline, BrandStory, Mission, Vision,
           AboutImagePath, HeroImagePath, UpdatedAt
    FROM   dbo.BusinessProfile
    WHERE  BusinessProfileId = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_BusinessProfile_Update
    @BusinessName   NVARCHAR(100),
    @Tagline        NVARCHAR(200) = NULL,
    @BrandStory     NVARCHAR(MAX) = NULL,
    @Mission        NVARCHAR(500) = NULL,
    @Vision         NVARCHAR(500) = NULL,
    @AboutImagePath NVARCHAR(260) = NULL,     -- NULL = keep current image
    @HeroImagePath  NVARCHAR(260) = NULL      -- NULL = keep current image
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@BusinessName, N'')))) = 0
        THROW 50001, 'Business name is required.', 1;

    IF EXISTS (SELECT 1 FROM dbo.BusinessProfile WHERE BusinessProfileId = 1)
        UPDATE dbo.BusinessProfile
        SET    BusinessName   = LTRIM(RTRIM(@BusinessName)),
               Tagline        = @Tagline,
               BrandStory     = @BrandStory,
               Mission        = @Mission,
               Vision         = @Vision,
               AboutImagePath = ISNULL(@AboutImagePath, AboutImagePath),
               HeroImagePath  = ISNULL(@HeroImagePath, HeroImagePath),
               UpdatedAt      = SYSDATETIME()
        WHERE  BusinessProfileId = 1;
    ELSE
        INSERT INTO dbo.BusinessProfile (BusinessName, Tagline, BrandStory, Mission, Vision, AboutImagePath, HeroImagePath)
        VALUES (LTRIM(RTRIM(@BusinessName)), @Tagline, @BrandStory, @Mission, @Vision, @AboutImagePath, @HeroImagePath);

    SELECT 1 AS Affected;
END
GO

/* ---------- ContactInfo ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_ContactInfo_Get
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ContactInfoId, Email, Phone, [Address], FacebookUrl, InstagramUrl, UpdatedAt
    FROM   dbo.ContactInfo
    WHERE  ContactInfoId = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ContactInfo_Update
    @Email        NVARCHAR(100) = NULL,
    @Phone        VARCHAR(20)   = NULL,
    @Address      NVARCHAR(250) = NULL,
    @FacebookUrl  NVARCHAR(250) = NULL,
    @InstagramUrl NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@Email)), N'') IS NOT NULL AND dbo.fn_IsValidEmail(LTRIM(RTRIM(@Email))) = 0     -- blank = no email
        THROW 50002, 'The business email address is not valid.', 1;

    IF EXISTS (SELECT 1 FROM dbo.ContactInfo WHERE ContactInfoId = 1)
        UPDATE dbo.ContactInfo
        SET    Email = NULLIF(LTRIM(RTRIM(@Email)), N''), Phone = @Phone, [Address] = @Address,
               FacebookUrl = @FacebookUrl, InstagramUrl = @InstagramUrl, UpdatedAt = SYSDATETIME()
        WHERE  ContactInfoId = 1;
    ELSE
        INSERT INTO dbo.ContactInfo (Email, Phone, [Address], FacebookUrl, InstagramUrl)
        VALUES (NULLIF(LTRIM(RTRIM(@Email)), N''), @Phone, @Address, @FacebookUrl, @InstagramUrl);

    SELECT 1 AS Affected;
END
GO

/* ---------- Owners ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Owner_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT OwnerId, FullName, Position, Bio, Email, PhotoPath, SortOrder
    FROM   dbo.Owners
    ORDER BY SortOrder, OwnerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Owner_GetById
    @OwnerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT OwnerId, FullName, Position, Bio, Email, PhotoPath, SortOrder
    FROM   dbo.Owners
    WHERE  OwnerId = @OwnerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Owner_Insert
    @FullName  NVARCHAR(100),
    @Position  NVARCHAR(100)  = NULL,
    @Bio       NVARCHAR(1000) = NULL,
    @Email     NVARCHAR(100)  = NULL,
    @PhotoPath NVARCHAR(260)  = NULL,
    @SortOrder INT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@FullName, N'')))) = 0
        THROW 50001, 'Owner name is required.', 1;

    INSERT INTO dbo.Owners (FullName, Position, Bio, Email, PhotoPath, SortOrder)
    VALUES (LTRIM(RTRIM(@FullName)), @Position, @Bio, @Email, @PhotoPath,
            ISNULL(@SortOrder, (SELECT ISNULL(MAX(SortOrder), 0) + 10 FROM dbo.Owners)));

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Owner_Update
    @OwnerId   INT,
    @FullName  NVARCHAR(100),
    @Position  NVARCHAR(100)  = NULL,
    @Bio       NVARCHAR(1000) = NULL,
    @Email     NVARCHAR(100)  = NULL,
    @PhotoPath NVARCHAR(260)  = NULL,     -- NULL = keep current photo
    @SortOrder INT            = NULL      -- NULL = keep current order
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@FullName, N'')))) = 0
        THROW 50001, 'Owner name is required.', 1;

    UPDATE dbo.Owners
    SET    FullName  = LTRIM(RTRIM(@FullName)),
           Position  = @Position,
           Bio       = @Bio,
           Email     = @Email,
           PhotoPath = ISNULL(@PhotoPath, PhotoPath),
           SortOrder = ISNULL(@SortOrder, SortOrder)
    WHERE  OwnerId = @OwnerId;

    SELECT @@ROWCOUNT AS Affected;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Owner_Delete
    @OwnerId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Owners WHERE OwnerId = @OwnerId;
    SELECT @@ROWCOUNT AS Affected;
END
GO

/* ---------- Products ---------- */
CREATE OR ALTER PROCEDURE dbo.usp_Product_GetAll          -- admin list: published and unpublished
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductId, ProductName, Flavor, ShortDescription, FullDescription, ImagePath,
           SortOrder, IsPublished, CreatedAt, UpdatedAt
    FROM   dbo.Products
    ORDER BY SortOrder, ProductId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_GetPublished    -- public catalogue: published only
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ProductId, ProductName, Flavor, ShortDescription, FullDescription, ImagePath,
           SortOrder, IsPublished, CreatedAt, UpdatedAt
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
           SortOrder, IsPublished, CreatedAt, UpdatedAt
    FROM   dbo.Products
    WHERE  ProductId = @ProductId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_Insert
    @ProductName      NVARCHAR(100),
    @Flavor           NVARCHAR(50)  = NULL,
    @ShortDescription NVARCHAR(250) = NULL,
    @FullDescription  NVARCHAR(MAX) = NULL,
    @ImagePath        NVARCHAR(260) = NULL,
    @SortOrder        INT           = NULL,
    @IsPublished      BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(LTRIM(RTRIM(ISNULL(@ProductName, N'')))) = 0
        THROW 50001, 'Product name is required.', 1;

    INSERT INTO dbo.Products (ProductName, Flavor, ShortDescription, FullDescription, ImagePath, SortOrder, IsPublished)
    VALUES (LTRIM(RTRIM(@ProductName)), NULLIF(LTRIM(RTRIM(@Flavor)), N''), @ShortDescription, @FullDescription, @ImagePath,
            ISNULL(@SortOrder, (SELECT ISNULL(MAX(SortOrder), 0) + 10 FROM dbo.Products)),
            ISNULL(@IsPublished, 1));

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
    @IsPublished      BIT           = NULL    -- NULL = keep current status
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
           UpdatedAt        = SYSDATETIME()
    WHERE  ProductId = @ProductId;

    SELECT @@ROWCOUNT AS Affected;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_SetPublished
    @ProductId   INT,
    @IsPublished BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Products
    SET    IsPublished = @IsPublished, UpdatedAt = SYSDATETIME()
    WHERE  ProductId = @ProductId;

    SELECT @@ROWCOUNT AS Affected;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_SetSortOrder
    @ProductId INT,
    @SortOrder INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Products
    SET    SortOrder = @SortOrder, UpdatedAt = SYSDATETIME()
    WHERE  ProductId = @ProductId;

    SELECT @@ROWCOUNT AS Affected;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Product_Delete
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Products WHERE ProductId = @ProductId;
    SELECT @@ROWCOUNT AS Affected;
END
GO

/* =====================================================================
   SEED DATA (guarded; safe to re-run)
   ===================================================================== */

/* Business profile: name and tagline are the brand's own public wording; the story is a placeholder. */
IF NOT EXISTS (SELECT 1 FROM dbo.BusinessProfile)
    INSERT INTO dbo.BusinessProfile (BusinessName, Tagline, BrandStory, Mission, Vision, AboutImagePath, HeroImagePath)
    VALUES (N'Mogu Mogu',
            N'The world''s favorite nata de coco drink!',
            N'[Placeholder - replace with the approved Mogu Mogu brand story in Admin > Business Profile.]',
            NULL, NULL, NULL, NULL);
GO

/* Contact information: intentionally empty. Do not invent contact details. */
IF NOT EXISTS (SELECT 1 FROM dbo.ContactInfo)
    INSERT INTO dbo.ContactInfo (Email, Phone, [Address], FacebookUrl, InstagramUrl)
    VALUES (NULL, NULL, NULL, NULL, NULL);
GO

/* Owners: one clearly marked placeholder to be replaced with the real, verified owner / company details. */
IF NOT EXISTS (SELECT 1 FROM dbo.Owners)
    INSERT INTO dbo.Owners (FullName, Position, Bio, Email, PhotoPath, SortOrder)
    VALUES (N'[Owner name - to be completed]', N'[Position - to be completed]',
            N'[Placeholder - enter the verified owner or company representative information in Admin > Owners.]',
            NULL, NULL, 10);
GO

/* Products: flavors listed by retailers of the Mogu Mogu range. No size, price or nutrition is claimed;
   images are added by the administrator. Review the list against the official product page. */
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
    INSERT INTO dbo.Products (ProductName, Flavor, ShortDescription, FullDescription, ImagePath, SortOrder, IsPublished)
    VALUES
        (N'Mogu Mogu Lychee', N'Lychee', N'Lychee flavoured drink with nata de coco.', NULL, NULL, 10, 1),
        (N'Mogu Mogu Mango',  N'Mango',  N'Mango flavoured drink with nata de coco.',  NULL, NULL, 20, 1),
        (N'Mogu Mogu Melon',  N'Melon',  N'Melon flavoured drink with nata de coco.',  NULL, NULL, 30, 1),
        (N'Mogu Mogu Peach',  N'Peach',  N'Peach flavoured drink with nata de coco.',  NULL, NULL, 40, 1);
GO
