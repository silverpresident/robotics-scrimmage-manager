-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RoboticsManager')
BEGIN
    CREATE DATABASE RoboticsManager;
END
GO

USE RoboticsManager;
GO

-- Create schema
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'robotics')
BEGIN
    EXEC('CREATE SCHEMA robotics');
END
GO

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

-- Teams table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[robotics].[Teams]') AND type in (N'U'))
BEGIN
    CREATE TABLE [robotics].[Teams] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Name] nvarchar(100) NOT NULL,
        [TeamNo] nvarchar(20) NOT NULL,
        [School] nvarchar(100) NOT NULL,
        [Color] nvarchar(7) NOT NULL,
        [LogoUrl] nvarchar(2048) NULL,
        [TotalPoints] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Teams_TeamNo] UNIQUE ([TeamNo])
    );
END
GO

-- Challenges table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[robotics].[Challenges]') AND type in (N'U'))
BEGIN
    CREATE TABLE [robotics].[Challenges] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Points] int NOT NULL,
        [IsUnique] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_Challenges] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Challenges_Name] UNIQUE ([Name])
    );
END
GO

-- ChallengeCompletions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[robotics].[ChallengeCompletions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [robotics].[ChallengeCompletions] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [TeamId] uniqueidentifier NOT NULL,
        [ChallengeId] uniqueidentifier NOT NULL,
        [PointsAwarded] int NOT NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_ChallengeCompletions] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_ChallengeCompletions_TeamChallenge] UNIQUE ([TeamId], [ChallengeId]),
        CONSTRAINT [FK_ChallengeCompletions_Teams] FOREIGN KEY ([TeamId]) REFERENCES [robotics].[Teams] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChallengeCompletions_Challenges] FOREIGN KEY ([ChallengeId]) REFERENCES [robotics].[Challenges] ([Id]) ON DELETE CASCADE
    );
END
GO

-- Announcements table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[robotics].[Announcements]') AND type in (N'U'))
BEGIN
    CREATE TABLE [robotics].[Announcements] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Body] nvarchar(max) NOT NULL,
        [RenderedBody] nvarchar(max) NULL,
        [Priority] int NOT NULL,
        [IsVisible] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id])
    );
END
GO

-- Updates table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[robotics].[Updates]') AND type in (N'U'))
BEGIN
    CREATE TABLE [robotics].[Updates] (
        [Id] uniqueidentifier NOT NULL DEFAULT NEWID(),
        [Type] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [TeamId] uniqueidentifier NULL,
        [ChallengeId] uniqueidentifier NULL,
        [AnnouncementId] uniqueidentifier NULL,
        [ChallengeCompletionId] uniqueidentifier NULL,
        [Metadata] nvarchar(max) NULL,
        [IsBroadcast] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_Updates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Updates_Teams] FOREIGN KEY ([TeamId]) REFERENCES [robotics].[Teams] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Updates_Challenges] FOREIGN KEY ([ChallengeId]) REFERENCES [robotics].[Challenges] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Updates_Announcements] FOREIGN KEY ([AnnouncementId]) REFERENCES [robotics].[Announcements] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Updates_ChallengeCompletions] FOREIGN KEY ([ChallengeCompletionId]) REFERENCES [robotics].[ChallengeCompletions] ([Id]) ON DELETE SET NULL
    );
END
GO

-- Create indexes
CREATE INDEX [IX_Teams_TotalPoints] ON [robotics].[Teams] ([TotalPoints] DESC);
CREATE INDEX [IX_ChallengeCompletions_TeamId] ON [robotics].[ChallengeCompletions] ([TeamId]);
CREATE INDEX [IX_ChallengeCompletions_ChallengeId] ON [robotics].[ChallengeCompletions] ([ChallengeId]);
CREATE INDEX [IX_Announcements_Priority_IsVisible] ON [robotics].[Announcements] ([Priority], [IsVisible]);
CREATE INDEX [IX_Updates_Type] ON [robotics].[Updates] ([Type]);
CREATE INDEX [IX_Updates_CreatedAt] ON [robotics].[Updates] ([CreatedAt] DESC);
GO

-- Insert default roles
IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [Name] = 'Administrator')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID());
END

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [Name] = 'Judge')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'Judge', 'JUDGE', NEWID());
END

IF NOT EXISTS (SELECT * FROM [dbo].[AspNetRoles] WHERE [Name] = 'Scorekeeper')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'Scorekeeper', 'SCOREKEEPER', NEWID());
END
GO
