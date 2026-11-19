/* =====================================================================
   MoguMogu_DB - PHASE 8 (OPTIONAL): RETIRE THE OLD PORTFOLIO TABLES
   Nothing in the application reads these tables or procedures any more, so skipping this script is safe.
   Run it only to leave a clean database for the demo.

   !! DESTRUCTIVE: the old portfolio rows (education, skills, hobbies, projects, gallery, personal info)
   !! are deleted permanently. Users, Posts, PageVisits and all Mogu Mogu business tables are NOT touched.

   Safety switch: nothing happens until you change @ConfirmRetire to 1 AND you have a backup/restore point.
   If you ever re-run GUADES_QuizScript_IPT102.sql afterwards, re-run GUADES_MoguMogu_ReportsUpgrade_IPT102.sql too.
   ===================================================================== */
USE MoguMogu_DB;
GO

DECLARE @ConfirmRetire BIT = 0;        -- <<< change to 1 to proceed

IF @ConfirmRetire <> 1
BEGIN
    PRINT 'Nothing was changed. Set @ConfirmRetire = 1 (after taking a backup) to retire the portfolio tables.';
    RETURN;
END

BEGIN TRY
    BEGIN TRANSACTION;

    DROP PROCEDURE IF EXISTS dbo.usp_PersonalInfo_Get,  dbo.usp_PersonalInfo_Update;
    DROP PROCEDURE IF EXISTS dbo.usp_Education_GetAll,  dbo.usp_Education_GetById, dbo.usp_Education_Insert, dbo.usp_Education_Update, dbo.usp_Education_Delete;
    DROP PROCEDURE IF EXISTS dbo.usp_Skill_GetAll,      dbo.usp_Skill_GetById,     dbo.usp_Skill_Insert,     dbo.usp_Skill_Update,     dbo.usp_Skill_Delete;
    DROP PROCEDURE IF EXISTS dbo.usp_Hobby_GetAll,      dbo.usp_Hobby_GetById,     dbo.usp_Hobby_Insert,     dbo.usp_Hobby_Update,     dbo.usp_Hobby_Delete;
    DROP PROCEDURE IF EXISTS dbo.usp_Project_GetAll,    dbo.usp_Project_GetById,   dbo.usp_Project_Insert,   dbo.usp_Project_Update,   dbo.usp_Project_Delete;
    DROP PROCEDURE IF EXISTS dbo.usp_Gallery_GetAll,    dbo.usp_Gallery_GetById,   dbo.usp_Gallery_Insert,   dbo.usp_Gallery_Update,   dbo.usp_Gallery_Delete;

    DROP TABLE IF EXISTS dbo.GalleryItems;
    DROP TABLE IF EXISTS dbo.Projects;
    DROP TABLE IF EXISTS dbo.Hobbies;
    DROP TABLE IF EXISTS dbo.Skills;
    DROP TABLE IF EXISTS dbo.Education;
    DROP TABLE IF EXISTS dbo.PersonalInfo;

    COMMIT TRANSACTION;
    PRINT 'Old portfolio tables and procedures were removed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
