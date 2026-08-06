resource "random_password" "sql_admin_password" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
  min_upper        = 1
  min_lower        = 1
  min_numeric      = 1
  min_special      = 1
}

resource "azurerm_key_vault_secret" "sql_admin_password_secret" {
  name         = "sql-admin-secret-${local.prefix}"
  value        = random_password.sql_admin_password.result
  key_vault_id = azurerm_key_vault.keyvault.id

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_server" "sql_server" {
  name                         = "sql-${local.prefix}"
  resource_group_name          = azurerm_resource_group.rg_main.name
  location                     = azurerm_resource_group.rg_main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = random_password.sql_admin_password.result
  tags                         = local.tags_sql

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_database" "sql_database" {
  name         = "sqldb-${local.prefix}"
  server_id    = azurerm_mssql_server.sql_server.id
  collation    = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb  = var.sql_max_size_gb
  sku_name     = var.sql_sku_name
  tags         = local.tags_sql

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_firewall_rule" "sql_firewall_rule" {
  name             = "sql-fw-${local.prefix}"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = var.local_ip_address
  end_ip_address   = var.local_ip_address
}

resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "sql-fw-azure-${local.prefix}"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "ConnectionStrings--DefaultConnection"
  value        = "Server=${azurerm_mssql_server.sql_server.fully_qualified_domain_name};Database=${azurerm_mssql_database.sql_database.name};User Id=${var.sql_admin_login};Password=${random_password.sql_admin_password.result};MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=False;"
  key_vault_id = azurerm_key_vault.keyvault.id

  lifecycle {
    prevent_destroy = true
  }
}