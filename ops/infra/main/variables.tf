variable "app_name" {
  description = "Name of the application"
  type        = string
}
variable "pipeline_sp_object_id" {
  description = "Pipeline service principal object ID for Key Vault access"
  type        = string
}
variable "current_user_object_id" {
  description = "Current user object ID for Key Vault access"
  type        = string
}

variable "environment" {
  description = "Deployment environment (e.g., dev, staging, prod)"
  type        = string
}
variable "aspnet_core_environment" {
  type = string
  description = "The ASP.NET Core environment (Development, Staging, Production)"
}

variable "location" {
  description = "Azure region for resource deployment"
  type        = string
}
variable "local_ip_address" {
  description = "Your local IP address to allow access to the SQL Server"
  type        = string
}

variable "log_analytics_sku" {
  description = "Log Analytics Workspace SKU"
  type        = string
  default     = "PerGB2018"
}
variable "log_analytics_retention_days" {
  description = "Log retention in days"
  type        = number
  default     = 30
}

variable "sql_admin_login" {
  description = "SQL Server administrator login"
  type        = string
}
variable "sql_max_size_gb" {
  description = "Maximum size of the SQL Database in GB"
  type        = number
}
variable "sql_sku_name" {
  description = "SKU name for the SQL Database (e.g., Basic, Standard, Premium)"
  type        = string
}

variable "app_service_os_type" {
  description = "Operating system for the App Service Plan"
  type        = string
  default     = "Linux"
}
variable "app_service_sku" {
  description = "SKU for the App Service Plan"
  type        = string
}
variable "app_service_dotnet_version" {
  description = "Version of .NET to use for the App Service"
  type        = string
}