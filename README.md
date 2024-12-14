# Robotics Scrimmage Manager

Web application for managing the ST JAGO ROBOTICS SCRIMMAGE 2024 competition.

## Features

- Real-time leaderboard and announcements using SignalR
- Team management and scoring system
- Challenge tracking and completion verification
- Role-based access control (Admin, Judge, Scorekeeper)
- Google authentication integration
- Mobile-responsive design with black and gold theme

## Requirements

- .NET 8.0 SDK
- SQL Server 2019+
- Azure subscription (for deployment)
- Google Developer Console project (for authentication)

## Quick Start

1. Configure your environment:
```bash
cp src/RoboticsManager.Web/appsettings.example.json src/RoboticsManager.Web/appsettings.json
# Edit appsettings.json with your settings
```

2. Set up the database:
```bash
sqlcmd -S your-server -i sql/setup.sql
sqlcmd -S your-server -i sql/seed.sql  # Optional: for test data
```

3. Run the application:
```bash
dotnet run --project src/RoboticsManager.Web
```

## Azure Deployment

1. Set up Terraform:
```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
```

2. Deploy infrastructure:
```bash
terraform init
terraform plan
terraform apply
```

3. Deploy application:
```bash
dotnet publish src/RoboticsManager.Web -c Release
```

## Project Structure

```
├── src/
│   ├── RoboticsManager.Lib/        # Core library
│   │   ├── Models/                 # Domain models
│   │   ├── Services/               # Business logic
│   │   ├── Data/                   # Database context
│   │   └── Hubs/                   # SignalR hubs
│   │
│   └── RoboticsManager.Web/        # Web application
│       ├── Controllers/            # MVC controllers
│       ├── Views/                  # Razor views
│       ├── wwwroot/               # Static files
│       └── Helpers/               # View helpers
│
├── sql/                           # Database scripts
├── terraform/                     # Infrastructure code
└── tests/                        # Unit tests
```

## Configuration

### Google Authentication

1. Create project in Google Cloud Console
2. Enable Google+ API
3. Create OAuth 2.0 credentials
4. Add authorized origins and redirect URIs
5. Update appsettings.json with credentials

### Azure Resources

Required resources:
- App Service (Web App)
- SQL Database
- Application Insights
- Key Vault (optional)

See terraform/terraform.tfvars.example for configuration options.

## Development Guidelines

- Use async/await for database operations
- Follow REST principles for API endpoints
- Implement proper error handling
- Add unit tests for new features
- Use SignalR for real-time updates
- Follow the existing code style

## License

MIT License
