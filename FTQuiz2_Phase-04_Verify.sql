/* FTQuiz2 Phase 04 verification - run in SSMS against the project database.
   Section 1 is read-only. Section 2 is for TESTING the pages: run a block, refresh the page, then run its
   "undo" line. Nothing here invents contact or owner details: replace every <...> with your own verified text.
   The admin editors for these records arrive in Phase 5, so until then SQL is how you change them. */
USE MoguMogu_DB;
GO

/* 1. What the four pages will read */
SELECT 'BusinessProfile (About)' AS Source, COUNT(*) AS N FROM dbo.BusinessProfile
UNION ALL SELECT 'ContactInfo (Contact)',          COUNT(*) FROM dbo.ContactInfo
UNION ALL SELECT 'Owners (Owners / Company)',      COUNT(*) FROM dbo.Owners
UNION ALL SELECT 'Products - published (listed)',  COUNT(*) FROM dbo.Products WHERE IsPublished = 1
UNION ALL SELECT 'Products - unpublished (hidden)',COUNT(*) FROM dbo.Products WHERE IsPublished = 0;

/* Product ids and their page address: /Products/<ProductId>. An unpublished id must show "Product not found". */
SELECT ProductId, ProductName, IsPublished, SortOrder, ImagePath,
       '/Products/' + CAST(ProductId AS varchar(10)) AS PageAddress
FROM dbo.Products ORDER BY SortOrder, ProductId;

/* Text still marked as a placeholder (shown with a visible "Placeholder" label on the pages) */
SELECT 'About story' AS Item, LEFT(BrandStory, 80) AS Value FROM dbo.BusinessProfile WHERE BrandStory LIKE N'[[]%]'
UNION ALL SELECT 'Owner', FullName FROM dbo.Owners WHERE FullName LIKE N'[[]%]' OR Bio LIKE N'[[]%]';
GO

/* 2. TEST BLOCKS (run one at a time) ------------------------------------------------------------------ */

/* 2a. Unpublished product: set a real ProductId, run, open /Products and /Products/<id>, then undo. */
-- UPDATE dbo.Products SET IsPublished = 0 WHERE ProductId = <ProductId>;     -- hidden from list, details = not found
-- UPDATE dbo.Products SET IsPublished = 1 WHERE ProductId = <ProductId>;     -- undo

/* 2b. Display order: a lower SortOrder is listed first. */
-- UPDATE dbo.Products SET SortOrder = 5 WHERE ProductId = <ProductId>;

/* 2c. About: replace the placeholder with the approved brand story (empty text hides that part of the page). */
-- UPDATE dbo.BusinessProfile SET BrandStory = N'<approved brand story>', Mission = N'<mission>', Vision = N'<vision>',
--        UpdatedAt = SYSDATETIME() WHERE BusinessProfileId = 1;

/* 2d. Contact: only filled values appear on /Contact. Links must start with http:// or https://. */
-- UPDATE dbo.ContactInfo SET Email = N'<verified email>', Phone = '<verified phone>', [Address] = N'<verified address>',
--        FacebookUrl = N'<https://...>', InstagramUrl = NULL, UpdatedAt = SYSDATETIME() WHERE ContactInfoId = 1;

/* 2e. Owners: replace the placeholder record with verified information (or add records). */
-- UPDATE dbo.Owners SET FullName = N'<verified name>', Position = N'<verified position>', Bio = N'<verified bio>',
--        Email = NULL WHERE OwnerId = <OwnerId>;
GO
