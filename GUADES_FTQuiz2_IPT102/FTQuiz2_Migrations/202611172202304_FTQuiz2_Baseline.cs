namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FTQuiz2_Baseline : DbMigration
    {
        public override void Up()
        {
            // Tables, columns, FKs and indexes already exist (created by GUADES_QuizScript_IPT102.sql).
            // The six CHECK constraints and the DESC index EF 6 cannot model are re-asserted, guarded, so a
            // database rebuilt from migrations matches the script.
            Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Role')
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role CHECK ([Role] IN ('Admin','User'));
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Status')
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Status CHECK ([Status] IN ('Active','Inactive'));
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_Phone')
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Phone CHECK (Phone NOT LIKE '%[^0-9]%' AND LEN(Phone) = 11);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Skills_Category')
    ALTER TABLE dbo.Skills ADD CONSTRAINT CK_Skills_Category CHECK (Category IN ('Frontend','Backend','Tools'));
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Education_Years')
    ALTER TABLE dbo.Education ADD CONSTRAINT CK_Education_Years CHECK (EndYear IS NULL OR EndYear >= StartYear);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PersonalInfo_SingleRow')
    ALTER TABLE dbo.PersonalInfo ADD CONSTRAINT CK_PersonalInfo_SingleRow CHECK (PersonalInfoId = 1);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Posts_IsPublished_CreatedAt' AND object_id = OBJECT_ID('dbo.Posts'))
    CREATE NONCLUSTERED INDEX IX_Posts_IsPublished_CreatedAt ON dbo.Posts (IsPublished, CreatedAt DESC);
");
        }

        public override void Down()
        {
            // Intentionally empty: these objects pre-date the baseline (they come from the SQL script),
            // so rolling the baseline back must not drop them.
        }
    }
}
