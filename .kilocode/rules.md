# .clinerules for the Robotics Scrimmage Manager project
# This file contains a set of rules for code analysis.
# The format is inspired by .editorconfig.

## Project Overview
This is a .NET 10.0 volleyball tournament management system with three main projects:
- **RoboticsManager.Lib**: Core business logic, models, services, and SignalR hubs
- **RoboticsManager.Web**: ASP.NET Core MVC admin application

## Technology Stack
- .NET 10.0
- ASP.NET Core MVC
- Blazor WebAssembly
- Entity Framework Core
- SQL Server
- SignalR for real-time updates
- Azure (App Services, SQL Database, SignalR Service, Key Vault, Application Insights)
- Terraform for infrastructure

## Code Organization Rules

### Project Structure
- Keep business logic in `RoboticsManager.Lib`
- Admin UI controllers belong in `RoboticsManager.Web/Areas/Admin/Controllers`
- Public API controllers belong in `RoboticsManager.Web/Controllers` with "Controller" suffix
- Models should be in appropriate `Models/` directories of the lib project
- Services must have interfaces and implementations (e.g., `ITournamentService` and `TournamentService`)
- Place interfaces in the Services\Interfaces folder
- Hubs should be in the Hubs folder


### Naming Conventions
- Use PascalCase for classes, methods, properties, and public members
- Use camelCase for private fields and local variables
- Prefix interface names with "I" (e.g., `ITournamentService`)
- Use descriptive names for variables and methods
- Controller actions should be named after HTTP verbs or actions (e.g., `Index`, `Create`, `Edit`, `Delete`)

### C# Coding Standards
- Always use `async`/`await` for asynchronous operations
- Use nullable reference types (`#nullable enable`)
- Prefer LINQ for collection operations
- Use dependency injection for all services
- Follow the Repository pattern for data access through DbContext
- Use `var` for local variables when type is obvious
- Always include XML documentation comments for public APIs
- Keep methods focused and under 50 lines when possible
- Do not embed styles in cshtml files, where necessary create a separate css file.
- Do not embed script in cshtml files, where necessary create a separate js file.
- Keep the README.md file updated
- Document the project using *.md files in the docs folder
- **Logging**: Use ASP.NET Core's logging framework for consistent logging throughout the application.

## Logging
- Add ILogger to all services and all controller.
- Log caught exceptions 

### Entity Framework & Database
- Database entities should inherit from `BaseEntity`
- Use Entity Framework Core migrations for schema changes
- Always use parameterized queries to prevent SQL injection
- Database initialization should use the `DatabaseInitialization` class
- Connection strings must be stored in appsettings.json (never hardcoded)
- Use `ApplicationDbContext` for all database operations
- Apply `.AsNoTracking()` for read-only queries to improve performance
- Use TSQL scripts for migrations and updates

### SignalR Real-Time Updates
- Use `TournamentHub` for broadcasting updates
- Notify clients after any score updates
- Hub methods should be minimal and delegate to services
- Use strongly-typed hub interfaces where possible

### Authentication & Authorization
- Use ASP.NET Core Identity for user management
- Support both Google and Microsoft OAuth providers
- Admin area requires authentication: `[Authorize]` attribute
- Public API endpoints should be anonymous: `[AllowAnonymous]`
- Store OAuth credentials in Azure Key Vault for production
- Never commit credentials to source control

### View Models & DTOs
- Create separate ViewModels for each view (in `ViewModels/` directories)
- Don't expose database entities directly to views
- Use AutoMapper or manual mapping for entity-to-ViewModel conversion
- ViewModels should only contain properties needed for the view
- Include validation attributes on ViewModel properties

### API Design
- API controllers should inherit from `ApiControllerBase`
- Use RESTful conventions for endpoint design
- Return appropriate HTTP status codes (200, 201, 400, 404, 500)
- Use `ActionResult<T>` for typed responses
- Include error handling with try-catch blocks
- Return JSON responses for all API endpoints
 

### Configuration Management
- Use appsettings.json for configuration (never hardcode values)
- Provide appsettings.Example.json with dummy values
- Store sensitive data in Azure Key Vault for production
- Use strongly-typed configuration classes (e.g., `DatabaseSettings`)
- Access configuration through dependency injection

### Error Handling
- Use try-catch blocks for all database operations
- Log errors using Application Insights
- Return user-friendly error messages to the UI
- Don't expose internal error details to public endpoints
- Use proper HTTP status codes for API errors

### Testing Guidelines
- Write unit tests for all service layer code
- Use mocking frameworks (e.g., Moq) for dependencies
- Test controllers with integration tests
- Include tests for SignalR hub methods
- Aim for 70%+ code coverage on business logic

## Azure Deployment

### Infrastructure as Code
- All Azure resources must be defined in Terraform
- Use `terraform/` directory for all infrastructure code
- Always run `terraform plan` before `terraform apply`
- Store Terraform state in Azure Storage backend
- Tag all resources with environment and project metadata

### Deployment Process
- Build in Release configuration for deployments
- Use `dotnet publish` for creating deployment packages
- Deploy Admin app and Public app to separate App Services
- Ensure connection strings are configured in Azure App Service settings
- Enable Application Insights for all deployed apps
- Use deployment slots for zero-downtime deployments

### Security Best Practices
- Enable HTTPS for all applications
- Store all secrets in Azure Key Vault
- Use Managed Identity for Azure resource access
- Enable Azure SQL firewall rules
- Keep all packages updated for security patches
- Use Azure AD B2C for production authentication
- Where appropriate POST action **must** be protected using the **Anti-Forgery Token** (`[ValidateAntiForgeryToken]`)

## File Organization

### When Creating New Files
- Controllers go in appropriate Areas/Controllers directories
- Views must match controller structure in Areas/Views
- Service interfaces go in `Services/Interfaces/I*.cs`
- Service implementations go in `Services/*Service.cs`
- Database entities go in `Lib/Models/*.cs`
- ViewModels go in `Lib/ViewModels/*.cs`
- Background and Hosted Service implementations go in `Workers/*Worker.cs`
- Hubs go in `Hubs/*Hub.cs`

### When Modifying Files
- Always read the entire file before making changes
- Maintain consistent formatting with existing code
- Update related files (e.g., if you modify a service, update its interface)
- Keep ViewModels synchronized with their corresponding views
- Update migrations when modifying entity models

## Common Patterns

### Service Pattern
```csharp
public interface ITournamentService
{
    Task<IEnumerable<Match>> GetAllMatchesAsync();
    Task<Match?> GetMatchByIdAsync(int id);
    Task CreateMatchAsync(Match match);
    Task UpdateMatchAsync(Match match);
}
```

### Controller Pattern
```csharp
[Authorize]
[Area("Admin")]
public class MatchesController : Controller
{
    private readonly ITournamentService _matchService;
    
    public MatchesController(ITournamentService matchService)
    {
        _matchService = matchService;
    }
    
    public async Task<IActionResult> Index()
    {
        var matches = await _matchService.GetAllMatchesAsync();
        return View(matches);
    }
}
```

### SignalR Notification Pattern
```csharp
// After updating a match
await _signalRService.SendMatchUpdateAsync(matchId, "Score updated");
```

## Testing Commands
- Run all tests: `dotnet test`
- Build solution: `dotnet build`
- Run migrations: `dotnet ef database update --project src/RoboticsManager.Lib --startup-project src/RoboticsManager.Web`
- Start Admin app: `cd src/RoboticsManager.Web && dotnet run`

## Important Notes
- Database setup script is in `SQL/setup.sql`
- Cleanup script is in `SQL/cleanup.sql`
- Never modify migrations after they've been applied
- Always test SignalR updates in both Admin and Public apps
- Keep API contracts consistent between Admin and Public apps
- The Public app refreshes automatically every 2 minutes
- Match updates should trigger immediate SignalR notifications
- The theme colour of the project is green and gold.
- Use DateTime.Now instead of DateTime.UtcNow