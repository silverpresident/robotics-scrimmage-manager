terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  backend "azurerm" {
    # Backend configuration will be provided via backend.tfvars
  }
}

provider "azurerm" {
  features {}
}

# Variables
variable "project_name" {
  description = "The name of the project"
  default     = "robotics-scrimmage"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  default     = "dev"
}

variable "location" {
  description = "The Azure region to deploy to"
  default     = "eastus"
}

variable "sql_admin_login" {
  description = "The admin username for SQL Server"
}

variable "sql_admin_password" {
  description = "The admin password for SQL Server"
  sensitive   = true
}

# Local variables
locals {
  resource_prefix = "${var.project_name}-${var.environment}"
  tags = {
    Environment = var.environment
    Project     = var.project_name
    ManagedBy   = "Terraform"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${local.resource_prefix}-rg"
  location = var.location
  tags     = local.tags
}

# Key Vault
resource "azurerm_key_vault" "main" {
  name                = "${local.resource_prefix}-kv"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  tenant_id          = data.azurerm_client_config.current.tenant_id
  sku_name           = "standard"

  tags = local.tags

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get", "List", "Create", "Delete", "Update",
    ]

    secret_permissions = [
      "Get", "List", "Set", "Delete",
    ]
  }
}

# SQL Server
resource "azurerm_mssql_server" "main" {
  name                         = "${local.resource_prefix}-sql"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  tags = local.tags
}

# SQL Database
resource "azurerm_mssql_database" "main" {
  name           = "${local.resource_prefix}-db"
  server_id      = azurerm_mssql_server.main.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 2
  sku_name       = "Basic"
  zone_redundant = false

  tags = local.tags
}

# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "${local.resource_prefix}-asp"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type            = "Windows"
  sku_name           = "B1"

  tags = local.tags
}

# App Service
resource "azurerm_windows_web_app" "main" {
  name                = "${local.resource_prefix}-app"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    always_on = true
    application_stack {
      dotnet_version = "v8.0"
    }
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.environment
    "AZURE_KEY_VAULT_ENDPOINT"             = azurerm_key_vault.main.vault_uri
    "ConnectionStrings__DefaultConnection"  = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_login};Password=${var.sql_admin_password};TrustServerCertificate=True"
    "WEBSITE_RUN_FROM_PACKAGE"             = "1"
  }

  tags = local.tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "${local.resource_prefix}-ai"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  application_type    = "web"

  tags = local.tags
}

# Store sensitive values in Key Vault
resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "SqlConnectionString"
  value        = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_login};Password=${var.sql_admin_password};TrustServerCertificate=True"
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "ApplicationInsightsConnectionString"
  value        = azurerm_application_insights.main.connection_string
  key_vault_id = azurerm_key_vault.main.id
}

# Data sources
data "azurerm_client_config" "current" {}

# Outputs
output "app_service_url" {
  value = "https://${azurerm_windows_web_app.main.default_hostname}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "application_insights_key" {
  value     = azurerm_application_insights.main.instrumentation_key
  sensitive = true
}
