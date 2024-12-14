# Azure Infrastructure Configuration

This directory contains Terraform configurations for deploying the Robotics Scrimmage Manager infrastructure to Azure.

## Infrastructure Components

- **Resource Group**: Contains all resources
- **App Service Plan**: Hosts the web application
- **Web App**: .NET 8.0 application
- **SQL Server**: Database server
- **SQL Database**: Application database
- **Application Insights**: Monitoring and analytics

## Prerequisites

1. Install required tools:
   - [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
   - [Terraform](https://www.terraform.io/downloads.html)

2. Login to Azure:
   ```bash
   az login
   ```

3. Configure Terraform:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   # Edit terraform.tfvars with your values
   ```

## Required Variables

| Variable | Description |
|----------|-------------|
| project_name | Project identifier for resource naming |
| environment | Deployment environment (dev/staging/prod) |
| location | Azure region for resources |
| sql_admin_login | SQL Server admin username |
| sql_admin_password | SQL Server admin password |
| google_client_id | Google OAuth client ID |
| google_client_secret | Google OAuth client secret |
| admin_email | System administrator email |

## Deployment Steps

1. Initialize Terraform:
   ```bash
   terraform init
   ```

2. Preview changes:
   ```bash
   terraform plan
   ```

3. Apply configuration:
   ```bash
   terraform apply
   ```

4. Verify deployment:
   ```bash
   terraform output webapp_url
   ```

## Resource Naming Convention

- Resource Group: `{project_name}-rg`
- App Service Plan: `{project_name}-plan`
- Web App: `{project_name}-web`
- SQL Server: `{project_name}-sql`
- SQL Database: `{project_name}-db`
- Application Insights: `{project_name}-insights`

## Cost Optimization

Default configuration uses:
- B1 App Service Plan (Basic tier)
- Basic SQL Database (2GB)
- Standard Application Insights

Modify `app_service_sku` and database settings in variables.tf for different tiers.

## Security Features

- HTTPS enforced
- TLS 1.2 required
- Azure services allowed to SQL Server
- Connection strings stored securely
- Managed identities enabled
- Network security rules applied

## Monitoring

Application Insights provides:
- Performance monitoring
- Usage analytics
- Error tracking
- Custom metrics
- Real-time monitoring

## Backup and Recovery

- SQL Database automatic backups
- Point-in-time restore capability
- Geo-redundant backup optional
- Configurable retention period

## Maintenance

To update infrastructure:
1. Modify configuration files
2. Run `terraform plan` to preview
3. Run `terraform apply` to implement

To destroy infrastructure:
```bash
terraform destroy
```

## Troubleshooting

Common issues:
1. Authentication errors: Check Azure CLI login
2. Resource conflicts: Check existing resources
3. Permission errors: Verify Azure permissions
4. Name conflicts: Ensure unique resource names
5. Quota limits: Check subscription limits

## Best Practices

- Use version control for Terraform files
- Keep secrets in Key Vault
- Use workspaces for environments
- Tag resources appropriately
- Review access controls regularly
- Monitor resource usage
- Implement cost controls
