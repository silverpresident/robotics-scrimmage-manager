# Database Scripts

This directory contains SQL scripts for setting up and managing the Robotics Scrimmage Manager database.

## Scripts Overview

### setup.sql
- Creates the database if it doesn't exist
- Sets up required tables and indexes
- Creates stored procedures for common operations
- Creates views for frequently used queries
- Configures database maintenance jobs

### seed.sql
- Populates the database with sample data for testing
- Includes example teams, challenges, and announcements
- Safe to run multiple times (cleans existing data first)
- Useful for development and testing environments

## Usage

### Initial Setup
```sql
-- Run as SQL Server administrator
sqlcmd -S your-server -i setup.sql
```

### Load Test Data
```sql
-- Optional: Run after setup.sql to load sample data
sqlcmd -S your-server -i seed.sql
```

## Database Objects

### Tables
- Teams
- Challenges
- ChallengeCompletions
- Announcements
- Updates
- AspNetUsers (Identity)

### Stored Procedures
- sp_GetTeamLeaderboard
- sp_GetChallengeStats
- sp_CleanupOldUpdates

### Views
- vw_ActiveAnnouncements
- vw_RecentUpdates

### Indexes
- IX_Teams_TotalPoints
- IX_Teams_TeamNo
- IX_Challenges_Points
- IX_Announcements_CreatedAt
- IX_Announcements_IsVisible
- IX_Updates_CreatedAt

## Maintenance

- Database backups are configured through Azure
- Automatic index maintenance is enabled
- Old updates are cleaned up after 30 days
- Statistics are automatically updated

## Security Notes

- Connection strings should be stored securely
- Use Azure Key Vault in production
- Follow principle of least privilege
- Enable TLS encryption for connections
- Regular security audits recommended

## Troubleshooting

If you encounter errors:
1. Ensure SQL Server is running
2. Verify connection string
3. Check permissions
4. Look for existing database/objects
5. Review SQL Server logs

## Development Guidelines

- Always use parameterized queries
- Include proper error handling
- Test scripts in development first
- Document schema changes
- Use transactions for data integrity
