variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "robotics-scrimmage"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod"
  }
}

variable "location" {
  description = "The Azure region to deploy to"
  type        = string
  default     = "eastus"
}

variable "sql_admin_login" {
  description = "The admin username for SQL Server"
  type        = string
  validation {
    condition     = length(var.sql_admin_login) >= 4
    error_message = "SQL admin login must be at least 4 characters"
  }
}

variable "sql_admin_password" {
  description = "The admin password for SQL Server"
  type        = string
  sensitive   = true
  validation {
    condition     = length(var.sql_admin_password) >= 8
    error_message = "SQL admin password must be at least 8 characters"
  }
}

variable "app_service_sku" {
  description = "The SKU for App Service Plan"
  type        = string
  default     = "B1"
}

variable "sql_database_sku" {
  description = "The SKU for SQL Database"
  type        = string
  default     = "Basic"
}

variable "key_vault_sku" {
  description = "The SKU for Key Vault"
  type        = string
  default     = "standard"
}

variable "tags" {
  description = "Additional tags for resources"
  type        = map(string)
  default     = {}
}
