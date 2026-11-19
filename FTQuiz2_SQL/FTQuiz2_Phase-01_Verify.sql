/* FTQuiz2 Phase 01 verification - run AFTER GUADES_QuizScript_IPT102.sql, then run the script again and re-run this. */
USE MoguMogu_DB;
GO
SELECT DB_NAME() AS CurrentDb,                                                        -- expect MoguMogu_DB
       (SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo'))           AS Tables_Expect9,
       (SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'usp[_]%')                AS Procs_Expect49,
       (SELECT COUNT(*) FROM sys.objects WHERE type = 'FN' AND name LIKE 'fn[_]%')    AS ScalarFns_Expect5,
       (SELECT COUNT(*) FROM sys.check_constraints)                                   AS Checks,
       (SELECT COUNT(*) FROM sys.indexes WHERE is_unique = 1 AND object_id IN (OBJECT_ID('dbo.Users'))) AS UsersUniqueIdx;
SELECT 'Users' T, COUNT(*) N FROM dbo.Users UNION ALL SELECT 'PersonalInfo', COUNT(*) FROM dbo.PersonalInfo
UNION ALL SELECT 'Education', COUNT(*) FROM dbo.Education UNION ALL SELECT 'Skills', COUNT(*) FROM dbo.Skills
UNION ALL SELECT 'Hobbies', COUNT(*) FROM dbo.Hobbies UNION ALL SELECT 'Projects', COUNT(*) FROM dbo.Projects
UNION ALL SELECT 'GalleryItems', COUNT(*) FROM dbo.GalleryItems UNION ALL SELECT 'Posts', COUNT(*) FROM dbo.Posts
UNION ALL SELECT 'PageVisits', COUNT(*) FROM dbo.PageVisits;
-- Rerun safety: row counts above must be identical after a second run of the main script.
