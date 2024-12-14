USE RoboticsManager;
GO

-- Disable constraints for seeding
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Clear existing data
DELETE FROM [dbo].[Updates];
DELETE FROM [dbo].[ChallengeCompletions];
DELETE FROM [dbo].[Challenges];
DELETE FROM [dbo].[Teams];
DELETE FROM [dbo].[Announcements];
GO

-- Seed initial teams
INSERT INTO [dbo].[Teams] (Id, Name, TeamNo, School, Color, LogoUrl, TotalPoints, CreatedAt)
VALUES
    (NEWID(), 'RoboWarriors', 'T001', 'St. Jago High School', '#FF0000', NULL, 0, GETUTCDATE()),
    (NEWID(), 'TechTitans', 'T002', 'Kingston College', '#0000FF', NULL, 0, GETUTCDATE()),
    (NEWID(), 'CircuitBreakers', 'T003', 'Wolmer''s Boys School', '#00FF00', NULL, 0, GETUTCDATE()),
    (NEWID(), 'ByteBots', 'T004', 'Campion College', '#FFFF00', NULL, 0, GETUTCDATE()),
    (NEWID(), 'DataDragons', 'T005', 'Jamaica College', '#800080', NULL, 0, GETUTCDATE());
GO

-- Seed initial challenges
INSERT INTO [dbo].[Challenges] (Id, Name, Description, Points, IsUnique, CreatedAt)
VALUES
    (NEWID(), 'Line Following', 
    '# Line Following Challenge\n\nNavigate your robot through a complex path marked by a black line on a white surface.\n\n## Requirements:\n- Complete the course within 2 minutes\n- Stay on the line for at least 90% of the course\n- Handle intersections correctly',
    100, 0, GETUTCDATE()),
    
    (NEWID(), 'Object Sorting', 
    '# Object Sorting Challenge\n\nProgram your robot to sort colored objects into corresponding bins.\n\n## Requirements:\n- Sort at least 5 objects correctly\n- Complete the task within 3 minutes\n- Handle objects of different sizes',
    150, 0, GETUTCDATE()),
    
    (NEWID(), 'Maze Navigation', 
    '# Maze Navigation Challenge\n\nGuide your robot through a complex maze using sensors.\n\n## Requirements:\n- Find the exit within 5 minutes\n- Avoid dead ends efficiently\n- Use wall-following or mapping algorithm',
    200, 1, GETUTCDATE()),
    
    (NEWID(), 'Precision Parking', 
    '# Precision Parking Challenge\n\nPark your robot in a designated spot with precise alignment.\n\n## Requirements:\n- Park within marked boundaries\n- Achieve correct orientation\n- Complete within 1 minute',
    75, 0, GETUTCDATE()),
    
    (NEWID(), 'Bridge Crossing', 
    '# Bridge Crossing Challenge\n\nCross a narrow bridge while maintaining balance.\n\n## Requirements:\n- Cross without falling\n- Maintain straight line\n- Complete within 30 seconds',
    125, 1, GETUTCDATE());
GO

-- Seed initial announcements
INSERT INTO [dbo].[Announcements] (Id, Body, Priority, IsVisible, CreatedAt)
VALUES
    (NEWID(), '# Welcome to ST JAGO ROBOTICS SCRIMMAGE 2024!\n\nWe''re excited to begin this amazing competition. Good luck to all teams!', 
    'Primary', 1, GETUTCDATE()),
    
    (NEWID(), '## Competition Schedule\n\n- 9:00 AM: Registration\n- 10:00 AM: Opening Ceremony\n- 10:30 AM: First Round\n- 12:00 PM: Lunch Break\n- 1:00 PM: Second Round\n- 3:00 PM: Finals\n- 4:00 PM: Awards Ceremony', 
    'Info', 1, GETUTCDATE()),
    
    (NEWID(), '**Safety Reminder**\n\nPlease ensure all robots comply with safety regulations:\n- No sharp edges\n- Voltage limit: 12V\n- Weight limit: 2kg', 
    'Warning', 1, GETUTCDATE()),
    
    (NEWID(), '### Judging Criteria\n\n1. Technical Innovation\n2. Code Quality\n3. Performance\n4. Documentation\n5. Team Collaboration', 
    'Info', 1, GETUTCDATE()),
    
    (NEWID(), '#### Practice Area Open\n\nThe practice area is now open for teams to test their robots. Please coordinate with other teams for time sharing.', 
    'Secondary', 1, GETUTCDATE());
GO

-- Re-enable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';

-- Update statistics
EXEC sp_updatestats;

PRINT 'Seed data inserted successfully.';
GO
