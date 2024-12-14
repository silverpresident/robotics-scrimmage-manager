# General variables
variable "project_name" {
  description = "The name of the project, used as a prefix for all resources"
  type        = string
  default     = "robotics-manager"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
  default     = "Production"
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "eastus"
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
  default     = "robotics-manager-rg"
}

# App Service variables
variable "app_service_sku" {
  description = "The SKU for the App Service Plan"
  type        = string
  default     = "B1"
}

# SQL Server variables
variable "sql_admin_login" {
  description = "The administrator username for the SQL Server"
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "The administrator password for the SQL Server"
  type        = string
  sensitive   = true
}

# Authentication variables
variable "google_client_id" {
  description = "The Google OAuth client ID"
  type        = string
  sensitive   = true
}

variable "google_client_secret" {
  description = "The Google OAuth client secret"
  type        = string
  sensitive   = true
}

variable "admin_email" {
  description = "The email address of the system administrator"
  type        = string
  sensitive   = true
}

# Tags
variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Environment = "Production"
    Project     = "Robotics Scrimmage Manager"
    Terraform   = "true"
  }
}

# Optional variables for scaling
variable "min_tls_version" {
  description = "The minimum supported TLS version"
  type        = string
  default     = "1.2"
}

variable "ftps_state" {
  description = "The FTPS state for the web app"
  type        = string
  default     = "Disabled"
}

variable "http2_enabled" {
  description = "Whether HTTP2 is enabled"
  type        = bool
  default     = true
}

variable "backup_retention_days" {
  description = "The number of days to retain database backups"
  type        = number
  default     = 7
}

variable "geo_redundant_backup_enabled" {
  description = "Whether geo-redundant backups are enabled"
  type        = bool
  default     = false
}
