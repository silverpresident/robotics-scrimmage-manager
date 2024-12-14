USE RoboticsManager;
GO

-- Disable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- Clear all data from tables in correct order
    DELETE FROM [robotics].[Updates];
    DELETE FROM [robotics].[ChallengeCompletions];
    DELETE FROM [robotics].[Teams];
    DELETE FROM [robotics].[Challenges];
    DELETE FROM [robotics].[Announcements];

    -- Clear Identity users and roles (optional - comment out if you want to keep users)
    DELETE FROM [dbo].[AspNetUserRoles];
    DELETE FROM [dbo].[AspNetUserClaims];
    DELETE FROM [dbo].[AspNetUserLogins];
    DELETE FROM [dbo].[AspNetUserTokens];
    DELETE FROM [dbo].[AspNetRoleClaims];
    DELETE FROM [dbo].[AspNetUsers];
    DELETE FROM [dbo].[AspNetRoles];

    -- Re-enable foreign key constraints
    EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

    -- Reinsert default roles
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES 
        (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID()),
        (NEWID(), 'Judge', 'JUDGE', NEWID()),
        (NEWID(), 'Scorekeeper', 'SCOREKEEPER', NEWID());

    COMMIT TRANSACTION;
    PRINT 'Database cleanup completed successfully.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error during cleanup: ' + ERROR_MESSAGE();
END CATCH
GO

-- Verify table counts
SELECT 'Updates' as [Table], COUNT(*) as [Count] FROM [robotics].[Updates]
UNION ALL
SELECT 'ChallengeCompletions', COUNT(*) FROM [robotics].[ChallengeCompletions]
UNION ALL
SELECT 'Teams', COUNT(*) FROM [robotics].[Teams]
UNION ALL
SELECT 'Challenges', COUNT(*) FROM [robotics].[Challenges]
UNION ALL
SELECT 'Announcements', COUNT(*) FROM [robotics].[Announcements]
UNION ALL
SELECT 'AspNetUsers', COUNT(*) FROM [dbo].[AspNetUsers]
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM [dbo].[AspNetRoles]
ORDER BY [Table];
GO

-- Print completion message
PRINT 'Database is now clean and ready for development.';
PRINT 'Run seed.sql to populate with sample data.';
GO
