/* OPTIONAL - run after you have copied your image files into the project.
   The admin editors that set these from the website arrive in Phase 5, so until then set image paths here.
   Paths are relative to the project folder, with forward slashes and no leading slash.
   Edit the file names to match the files you actually saved. A path whose file does not exist is simply
   ignored by the site (a placeholder is shown), so nothing breaks if a name is wrong. */
USE MoguMogu_DB;
GO
UPDATE dbo.BusinessProfile SET HeroImagePath = N'FTQuiz2_Media/Brand/mogu-mogu-hero.jpg', UpdatedAt = SYSDATETIME() WHERE BusinessProfileId = 1;

UPDATE dbo.Products SET ImagePath = N'FTQuiz2_Media/Products/product-lychee.png'  WHERE ProductName = N'Mogu Mogu Lychee';
UPDATE dbo.Products SET ImagePath = N'FTQuiz2_Media/Products/product-mango.png'   WHERE ProductName = N'Mogu Mogu Mango';
UPDATE dbo.Products SET ImagePath = N'FTQuiz2_Media/Products/product-melon.png'   WHERE ProductName = N'Mogu Mogu Melon';
UPDATE dbo.Products SET ImagePath = N'FTQuiz2_Media/Products/product-peach.png'   WHERE ProductName = N'Mogu Mogu Peach';
GO
SELECT ProductName, ImagePath, IsPublished FROM dbo.Products ORDER BY SortOrder;
