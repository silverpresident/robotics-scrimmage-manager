# ST JAGO ROBOTICS SCRIMMAGE MANAGER

A comprehensive management system for the St. Jago Robotics Scrimmage 2024 competition. The system includes both an administrative interface for managing teams, challenges, and announcements, as well as public displays for leaderboards and real-time updates.

## Architecture

The solution is structured into two main projects:

### RoboticsManager.Lib
Core business logic library containing:
- Entity models and database context
- Business services
- Real-time update system using SignalR
- Configuration management
- Database initialization and seeding

### RoboticsManager.Web
MVC web application that provides:
- Administrative interface
- Public displays
- User authentication/authorization
- Real-time updates

## Features

### Team Management
- Create, update, and delete teams
- Track team points
- Upload team logos
- View team completion history
- Real-time leaderboard updates

### Challenge Management
- Create and manage challenges
- Support for unique (first-to-complete) challenges
- Track challenge completions
- Award points automatically
- Challenge completion statistics

### Announcements
- Priority-based announcements (Info, Warning, Danger, etc.)
- Markdown support for rich content
- Show/hide functionality
- Automatic announcements for significant events
- Real-time updates

### Real-time Updates
- SignalR-based real-time notifications
- Leaderboard updates
- Challenge completion broadcasts
- Announcement pushes
- Automatic page refresh every 5 minutes

### Security
- Google Single Sign-On integration
- Role-based access control
- Audit logging of all changes
- Secure configuration management

## Technology Stack

- .NET 8.0
- Entity Framework Core
- SQL Server
- SignalR for real-time updates
- Markdig for Markdown processing
- Azure hosting support

## Getting Started

1. Clone the repository:
```powershell
git clone https://github.com/your-org/robotics-scrimmage-manager.git
cd robotics-scrimmage-manager
```

2. Create configuration files:
```powershell
cp src/RoboticsManager.Web/appsettings.example.json src/RoboticsManager.Web/appsettings.json
```

3. Update the configuration in `appsettings.json` with your settings:
- Database connection string
- Google authentication credentials
- Application settings

4. Create and update the database:
```powershell
cd src/RoboticsManager.Web
dotnet ef database update
```

5. Run the application:
```powershell
dotnet run
```

## Azure Deployment

The project includes Terraform configurations for Azure deployment. See the [Terraform README](terraform/README.md) for detailed deployment instructions.

1. Configure Azure resources:
```powershell
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Update terraform.tfvars with your values
```

2. Deploy to Azure:
```powershell
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

## Configuration Options

### Competition Settings
- Name and branding
- Refresh intervals
- Leaderboard size
- Announcement counts

### Theme Customization
- Primary and secondary colors
- Font family
- Logo URL

### SignalR Options
- Hub configuration
- Connection settings
- Message size limits

### Database Options
- Command timeouts
- Retry policies
- Logging settings

See `appsettings.example.json` for all available options.

## Development

### Adding Migrations
```powershell
cd src/RoboticsManager.Web
dotnet ef migrations add MigrationName --project ../RoboticsManager.Lib
dotnet ef database update
```

### Running Tests
```powershell
dotnet test
```

### Code Style
- Follow .NET coding conventions
- Use async/await consistently
- Document public APIs
- Include XML comments for public members

## Services

### ITeamService
- Team management and points tracking
- Leaderboard functionality
- Challenge completion tracking

### IChallengeService
- Challenge management
- Completion tracking
- Unique challenge handling
- Statistics and reporting

### IAnnouncementService
- Announcement management
- Markdown processing
- Priority-based filtering
- Automatic announcements

### IUpdateService
- Real-time update broadcasting
- Change tracking
- Audit logging
- Event notifications

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues:
- Check existing GitHub issues
- Review the documentation
- Submit a new issue with:
  - Clear description
  - Steps to reproduce
  - Expected vs actual behavior
