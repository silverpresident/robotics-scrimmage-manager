# Infrastructure Deployment with Terraform

This directory contains Terraform configurations for deploying the Robotics Scrimmage Manager infrastructure to Azure.

## Prerequisites

1. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. [Terraform](https://www.terraform.io/downloads.html) (version >= 1.0.0)
3. Azure subscription with required permissions
4. Service Principal with Contributor access

## Azure Resources Created

- Resource Group
- Azure SQL Server and Database
- App Service Plan and App Service
- Key Vault
- Application Insights

## Configuration

1. Create a `terraform.tfvars` file with your specific values:

```hcl
project_name       = "robotics-scrimmage"
environment        = "dev"  # or "staging" or "prod"
location          = "eastus"
sql_admin_login   = "your-admin-username"
sql_admin_password = "your-secure-password"

# Optional overrides
app_service_sku   = "B1"    # Default: "B1"
sql_database_sku  = "Basic" # Default: "Basic"
key_vault_sku     = "standard" # Default: "standard"

tags = {
  Owner       = "Your Name"
  Department  = "Your Department"
}
```

2. Create a `backend.tfvars` file for the Azure Storage backend:

```hcl
resource_group_name  = "terraform-state-rg"
storage_account_name = "tfstate12345"
container_name       = "tfstate"
key                 = "robotics-scrimmage.tfstate"
```

## Authentication

1. Log in to Azure:
```bash
az login
```

2. Set your subscription:
```bash
az account set --subscription "Your Subscription Name"
```

## Deployment Steps

1. Initialize Terraform:
```bash
terraform init -backend-config=backend.tfvars
```

2. Plan the deployment:
```bash
terraform plan -var-file=terraform.tfvars -out=tfplan
```

3. Apply the configuration:
```bash
terraform apply tfplan
```

## Post-Deployment

After deployment, you'll need to:

1. Configure the application settings in the Azure Portal or update your CI/CD pipeline:
   - Update the Key Vault access policies
   - Configure Google authentication
   - Set up any additional application settings

2. Get the deployment outputs:
```bash
terraform output
```

This will show:
- App Service URL
- SQL Server FQDN
- Key Vault URI
- Application Insights Key

## Cleanup

To remove all resources:
```bash
terraform destroy -var-file=terraform.tfvars
```

## Important Notes

1. The SQL Server firewall is configured to allow Azure services by default. You may need to add your IP address for local development.

2. Key Vault secrets are created for:
   - SQL Connection String
   - Application Insights Connection String

3. The App Service is configured with:
   - .NET 8.0
   - Always On enabled
   - Managed identity for Key Vault access

## Cost Considerations

The default configuration uses:
- Basic (B1) App Service Plan
- Basic SQL Database
- Standard Key Vault

Estimated monthly cost: ~$60-80 USD (varies by region and usage)

To reduce costs for development:
- Use Free tier App Service Plan (F1) - Remove "always_on" setting
- Use Basic SQL Database with lower DTUs
- Delete resources when not in use

## Security Notes

1. All sensitive values should be stored in Key Vault
2. SQL Server passwords should be rotated regularly
3. Enable Azure AD authentication for SQL Server in production
4. Review and restrict Key Vault access policies
5. Enable HTTPS-only for the App Service
6. Configure backup policies for the database

## Troubleshooting

1. If deployment fails:
   - Check Azure permissions
   - Verify resource name availability
   - Review resource quotas
   - Check resource dependencies

2. Common issues:
   - Name conflicts (resources must be globally unique)
   - Insufficient permissions
   - Resource provider not registered
   - Region availability for services

## Support

For issues with:
- Infrastructure deployment: Review Azure Portal activity logs
- Terraform configuration: Check Terraform plan output
- Application configuration: Review App Service logs
