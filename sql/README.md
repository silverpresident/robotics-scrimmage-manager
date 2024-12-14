# Database Scripts

This directory contains SQL scripts for setting up and managing the Robotics Scrimmage Manager database.

## Scripts Overview

1. `setup.sql`: Creates the database schema
   - Creates database if it doesn't exist
   - Sets up required tables and relationships
   - Creates indexes for performance
   - Adds default roles

2. `seed.sql`: Populates the database with sample data
   - Sample teams
   - Sample challenges
   - Sample announcements
   - Safe to run multiple times (cleans existing data first)

3. `cleanup.sql`: Removes all data for development
   - Keeps schema intact
   - Removes all data from tables
   - Resets identity columns

## Usage

### Initial Setup

1. Connect to your SQL Server instance
2. Run the setup script:
```sql
sqlcmd -S .\SQLEXPRESS -i setup.sql
```

### Adding Sample Data

After setup, you can add sample data:
```sql
sqlcmd -S .\SQLEXPRESS -i seed.sql
```

### Development Cleanup

To reset the database (keeping schema):
```sql
sqlcmd -S .\SQLEXPRESS -i cleanup.sql
```

## Database Schema

### Tables

1. `[robotics].[Teams]`
   - Core team information
   - Tracks points and achievements
   - Links to team logo

2. `[robotics].[Challenges]`
   - Challenge definitions
   - Points and requirements
   - Unique vs regular challenges

3. `[robotics].[ChallengeCompletions]`
   - Records of completed challenges
   - Points awarded
   - Completion notes

4. `[robotics].[Announcements]`
   - System announcements
   - Priority levels
   - Markdown content

5. `[robotics].[Updates]`
   - System-wide activity log
   - Links to related entities
   - Broadcast status

6. `[dbo].[AspNetUsers]` and related tables
   - Identity framework tables
   - User authentication
   - Role management

### Key Relationships

```plaintext
Teams ─┬─── ChallengeCompletions ───┬─ Challenges
       └─── Updates ────────────────┘
                      └─── Announcements
```

## Important Notes

1. **Permissions**
   - Scripts assume you have database creation rights
   - Requires ALTER, INSERT, DELETE permissions
   - Should be run as database owner or administrator

2. **Data Reset**
   - `seed.sql` clears existing data before inserting
   - Use with caution in production
   - Consider backing up data first

3. **Identity Tables**
   - Standard ASP.NET Core Identity tables
   - Located in `dbo` schema
   - Managed by Entity Framework migrations

4. **Indexes**
   - Optimized for common queries
   - Consider monitoring for additional needs
   - Adjust based on actual usage patterns

## Development Workflow

1. First-time setup:
```bash
sqlcmd -S .\SQLEXPRESS -i setup.sql
sqlcmd -S .\SQLEXPRESS -i seed.sql
```

2. Reset during development:
```bash
sqlcmd -S .\SQLEXPRESS -i cleanup.sql
sqlcmd -S .\SQLEXPRESS -i seed.sql
```

3. Update schema:
   - Use Entity Framework migrations
   - Or create new SQL scripts
   - Document changes in this README

## Troubleshooting

1. **Permission Errors**
   - Verify SQL Server authentication
   - Check user permissions
   - Run as administrator if needed

2. **Data Issues**
   - Check foreign key constraints
   - Verify data integrity
   - Use cleanup script if needed

3. **Performance**
   - Monitor index usage
   - Check query plans
   - Adjust indexes as needed

## Best Practices

1. Always backup before running scripts
2. Test in development first
3. Use transactions for safety
4. Monitor script execution time
5. Keep scripts idempotent
6. Document any schema changes
