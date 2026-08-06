output "app_service_name" {
  value = azurerm_linux_web_app.app_service.name
}

output "app_service_url" {
  value = "https://${azurerm_linux_web_app.app_service.default_hostname}"
}

output "sql_server_name" {
  value = azurerm_mssql_server.sql_server.name
}

output "key_vault_name" {
  value = azurerm_key_vault.keyvault.name
}

output "app_insights_connection_string" {
  value     = azurerm_application_insights.app_insights.connection_string
  sensitive = true
}