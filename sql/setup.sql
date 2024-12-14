-- Create database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RoboticsManager')
BEGIN
    CREATE DATABASE RoboticsManager;
END
GO

USE RoboticsManager;
GO

-- Enable GUID compression for better performance with EntityFramework
ALTER DATABASE RoboticsManager
SET COMPRESSION_DELAY = 0;
GO

-- Create AspNetIdentity tables if they don't exist

-- AspNetRoles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END
GO

-- AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [FullName] nvarchar(100) NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END
GO

-- Create custom indexes for better query performance
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Teams]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_TotalPoints' AND object_id = OBJECT_ID('Teams'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Teams_TotalPoints] ON [dbo].[Teams] ([TotalPoints] DESC);
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Teams_TeamNo' AND object_id = OBJECT_ID('Teams'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Teams_TeamNo] ON [dbo].[Teams] ([TeamNo]);
    END
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Challenges]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Challenges_Points' AND object_id = OBJECT_ID('Challenges'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Challenges_Points] ON [dbo].[Challenges] ([Points] DESC);
    END
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Announcements]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Announcements_CreatedAt' AND object_id = OBJECT_ID('Announcements'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Announcements_CreatedAt] ON [dbo].[Announcements] ([CreatedAt] DESC);
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Announcements_IsVisible' AND object_id = OBJECT_ID('Announcements'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Announcements_IsVisible] ON [dbo].[Announcements] ([IsVisible]);
    END
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Updates]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Updates_CreatedAt' AND object_id = OBJECT_ID('Updates'))
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_Updates_CreatedAt] ON [dbo].[Updates] ([CreatedAt] DESC);
    END
END

-- Create stored procedures for common operations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetTeamLeaderboard]') AND type in (N'P'))
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_GetTeamLeaderboard]
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            t.Id,
            t.Name,
            t.TeamNo,
            t.School,
            t.TotalPoints,
            COUNT(cc.Id) AS CompletedChallenges
        FROM Teams t
        LEFT JOIN ChallengeCompletions cc ON t.Id = cc.TeamId
        GROUP BY t.Id, t.Name, t.TeamNo, t.School, t.TotalPoints
        ORDER BY t.TotalPoints DESC;
    END
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetChallengeStats]') AND type in (N'P'))
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_GetChallengeStats]
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            c.Id,
            c.Name,
            c.Points,
            c.IsUnique,
            COUNT(cc.Id) AS CompletionCount,
            MIN(cc.CreatedAt) AS FirstCompletion
        FROM Challenges c
        LEFT JOIN ChallengeCompletions cc ON c.Id = cc.ChallengeId
        GROUP BY c.Id, c.Name, c.Points, c.IsUnique
        ORDER BY CompletionCount DESC;
    END
    ');
END
GO

-- Create views for common queries
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ActiveAnnouncements]'))
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_ActiveAnnouncements]
    AS
    SELECT 
        Id,
        Body,
        RenderedBody,
        Priority,
        CreatedAt,
        UpdatedAt
    FROM Announcements
    WHERE IsVisible = 1
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_RecentUpdates]'))
BEGIN
    EXEC('
    CREATE VIEW [dbo].[vw_RecentUpdates]
    AS
    SELECT TOP 100
        Id,
        Description,
        Type,
        CreatedAt,
        ReferenceId
    FROM Updates
    ORDER BY CreatedAt DESC
    ');
END
GO

-- Set up database maintenance jobs
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CleanupOldUpdates]') AND type in (N'P'))
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_CleanupOldUpdates]
        @DaysToKeep int = 30
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DELETE FROM Updates
        WHERE CreatedAt < DATEADD(day, -@DaysToKeep, GETUTCDATE());
    END
    ');
END
GO

PRINT 'Database setup completed successfully.';
GO
