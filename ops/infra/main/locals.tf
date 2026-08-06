locals {
  prefix = "${var.app_name}-${var.environment}"

  common_tags = {
    environment = var.environment
    project     = "ipnoticehub"
    managed_by  = "terraform"
    owner       = "nikolay_todorov"
  }

  tags_app_service = merge(local.common_tags, {
    tier        = "frontend"
    runtime     = "dotnet-8"
    purpose     = "web-app"
    criticality = "medium"
  })

  tags_app_service_plan = merge(local.common_tags, {
    tier        = "compute"
    runtime     = "linux"
    purpose     = "app-hosting"
    criticality = "medium"
  })

  tags_sql = merge(local.common_tags, {
    tier        = "data"
    backup      = "required"
    purpose     = "database"
    criticality = "high"
  })

  tags_key_vault = merge(local.common_tags, {
    tier        = "security"
    purpose     = "secrets"
    criticality = "high"
  })

  tags_monitoring = merge(local.common_tags, {
    tier        = "monitoring"
    purpose     = "observability"
    criticality = "medium"
  })
}