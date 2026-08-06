resource "azurerm_log_analytics_workspace" "log_analytics" {
  name                = "log-${local.prefix}"
  location            = azurerm_resource_group.rg_main.location
  resource_group_name = azurerm_resource_group.rg_main.name
  sku                 = var.log_analytics_sku
  retention_in_days   = var.log_analytics_retention_days
  tags                = local.tags_monitoring
}

resource "azurerm_application_insights" "app_insights" {
  name                = "appi-${local.prefix}"
  resource_group_name = azurerm_resource_group.rg_main.name
  location            = azurerm_resource_group.rg_main.location
  workspace_id        = azurerm_log_analytics_workspace.log_analytics.id
  application_type    = "web"
  tags                = local.tags_monitoring
}

resource "azurerm_monitor_diagnostic_setting" "app_diagnostic" {
  name                       = "diag-app-${local.prefix}"
  target_resource_id         = azurerm_linux_web_app.app_service.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics.id

  enabled_log {
    category = "AppServiceHTTPLogs"
  }

  enabled_log {
    category = "AppServiceAppLogs"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}