data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "keyvault" {
  name                        = "kv-${local.prefix}"
  location                    = azurerm_resource_group.rg_main.location
  resource_group_name         = azurerm_resource_group.rg_main.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  public_network_access_enabled = true
  rbac_authorization_enabled = true
  purge_protection_enabled    = false
  tags                        = local.tags_key_vault

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_monitor_diagnostic_setting" "keyvault_diagnostics" {
  name               = "kv-diag-${local.prefix}"
  target_resource_id = azurerm_key_vault.keyvault.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics.id

  enabled_log {
    category = "AuditEvent"
  }

  enabled_metric {
    category = "AllMetrics"
  }
}
