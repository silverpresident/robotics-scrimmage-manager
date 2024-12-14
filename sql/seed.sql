USE RoboticsManager;
GO

-- Disable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Delete existing data
DELETE FROM [robotics].[Updates];
DELETE FROM [robotics].[ChallengeCompletions];
DELETE FROM [robotics].[Teams];
DELETE FROM [robotics].[Challenges];
DELETE FROM [robotics].[Announcements];
GO

-- Sample Teams
INSERT INTO [robotics].[Teams] ([Id], [Name], [TeamNo], [School], [Color], [LogoUrl], [TotalPoints])
VALUES
    (NEWID(), 'RoboWarriors', 'T001', 'St. Jago High School', '#FF0000', '/images/teams/robowarriors.png', 0),
    (NEWID(), 'TechTitans', 'T002', 'Kingston College', '#0000FF', '/images/teams/techtitans.png', 0),
    (NEWID(), 'CircuitBreakers', 'T003', 'Wolmers Boys School', '#00FF00', '/images/teams/circuitbreakers.png', 0),
    (NEWID(), 'ByteBots', 'T004', 'Campion College', '#FFFF00', '/images/teams/bytebots.png', 0),
    (NEWID(), 'DataDragons', 'T005', 'Jamaica College', '#800080', '/images/teams/datadragons.png', 0);
GO

-- Sample Challenges
INSERT INTO [robotics].[Challenges] ([Id], [Name], [Description], [Points], [IsUnique])
VALUES
    (NEWID(), 'Line Following', 
    '# Line Following Challenge

Navigate your robot through a complex line course with intersections and curves.

## Requirements
- Robot must follow the black line
- Complete the course within 3 minutes
- Handle at least two intersections
- Navigate curves smoothly

## Scoring
- Base points: 100
- Bonus points for speed (up to 50)
- Penalty for leaving the line (-10 each time)', 
    100, 0),

    (NEWID(), 'Maze Navigation', 
    '# Maze Navigation Challenge

Guide your robot through a maze using sensors and autonomous control.

## Requirements
- Robot must be fully autonomous
- Find the exit within 5 minutes
- Handle dead ends correctly
- No manual intervention allowed

## Scoring
- Base points: 150
- Bonus for optimal path (50)
- Time penalties after 3 minutes', 
    150, 0),

    (NEWID(), 'Object Sorting',
    '# Object Sorting Challenge

Sort colored objects into designated zones using computer vision.

## Requirements
- Identify red, blue, and green objects
- Place objects in matching zones
- Complete task within 4 minutes
- Minimum 90% accuracy

## Scoring
- Base points: 200
- Bonus per perfect sort (10)
- Penalty for misplaced objects (-20)',
    200, 0),

    (NEWID(), 'First to Cross',
    '# First to Cross Challenge

Be the first team to successfully cross the advanced obstacle course.

## Requirements
- Navigate all obstacles
- Maintain balance
- Complete course within 5 minutes
- No manual intervention

## Scoring
- First team only: 300 points
- Must complete entire course
- No partial points awarded',
    300, 1);
GO

-- Sample Announcements
INSERT INTO [robotics].[Announcements] ([Id], [Body], [Priority], [IsVisible])
VALUES
    (NEWID(), 
    '# Welcome to ST JAGO ROBOTICS SCRIMMAGE 2024!

Get ready for an exciting day of robotics challenges and competition. Good luck to all participating teams!

## Important Information
- Please check in at the registration desk
- Safety briefing starts at 9:00 AM
- Practice rounds begin at 10:00 AM',
    0, -- Info
    1),

    (NEWID(),
    '⚠️ **Safety Reminder**

Please ensure all robots comply with safety regulations:
- Maximum voltage: 12V
- Emergency stop button required
- No sharp edges or exposed wires
- Weight limit: 3kg',
    1, -- Warning
    1),

    (NEWID(),
    '🏆 **New Challenge Available**

The "First to Cross" challenge is now open! Be the first team to complete it and earn 300 points!

- Location: Main Arena
- Time Limit: 5 minutes
- Unique Challenge: Points awarded to first successful completion only',
    2, -- Primary
    1);
GO

-- Re-enable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
GO

-- Update rendered content for announcements
UPDATE [robotics].[Announcements]
SET [RenderedBody] = [Body];
GO
