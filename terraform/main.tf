# Configure Azure provider
provider "azurerm" {
  features {}
}

# Create resource group
resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

# Create App Service Plan
resource "azurerm_service_plan" "app_plan" {
  name                = "${var.project_name}-plan"
  resource_group_name = azurerm_resource_group.rg.name
  location           = azurerm_resource_group.rg.location
  os_type            = "Windows"
  sku_name           = var.app_service_sku

  tags = var.tags
}

# Create Web App
resource "azurerm_windows_web_app" "web_app" {
  name                = "${var.project_name}-web"
  resource_group_name = azurerm_resource_group.rg.name
  location           = azurerm_resource_group.rg.location
  service_plan_id    = azurerm_service_plan.app_plan.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    always_on = true
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"              = var.environment
    "WEBSITE_RUN_FROM_PACKAGE"            = "1"
    "Authentication:Google:ClientId"       = var.google_client_id
    "Authentication:Google:ClientSecret"   = var.google_client_secret
    "RoboticsManager:AdminEmail"          = var.admin_email
  }

  connection_strings {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }

  tags = var.tags
}

# Create SQL Server
resource "azurerm_mssql_server" "sql" {
  name                         = "${var.project_name}-sql"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  tags = var.tags
}

# Create SQL Database
resource "azurerm_mssql_database" "db" {
  name           = "${var.project_name}-db"
  server_id      = azurerm_mssql_server.sql.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 2
  sku_name       = "Basic"
  zone_redundant = false

  tags = var.tags
}

# Allow Azure services to access SQL Server
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Create Application Insights
resource "azurerm_application_insights" "insights" {
  name                = "${var.project_name}-insights"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"

  tags = var.tags
}

# Output values
output "webapp_url" {
  value = "https://${azurerm_windows_web_app.web_app.default_hostname}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "application_insights_key" {
  value     = azurerm_application_insights.insights.instrumentation_key
  sensitive = true
}
