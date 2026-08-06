resource "azurerm_service_plan" "app_service_plan" {
  name                = "asp-${local.prefix}"
  resource_group_name = azurerm_resource_group.rg_main.name
  location            = azurerm_resource_group.rg_main.location
  os_type             = var.app_service_os_type
  sku_name            = var.app_service_sku
  tags                = local.tags_app_service_plan
}

resource "azurerm_linux_web_app" "app_service" {
  name                = "app-${local.prefix}"
  resource_group_name = azurerm_resource_group.rg_main.name
  location            = azurerm_service_plan.app_service_plan.location
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  tags                = local.tags_app_service
  https_only          = true

  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on                               = true
    container_registry_use_managed_identity = true
    http2_enabled                           = true
    ftps_state                              = "Disabled"
    health_check_path                       = "/health"
    health_check_eviction_time_in_min       = 5

    application_stack {
      dotnet_version = var.app_service_dotnet_version
    }

    auto_heal_setting {
      action {
        action_type = "Recycle"
      }
      trigger {
        requests {
          count    = 50
          interval = "00:01:00"
        }
      }
    }
  }

app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.aspnet_core_environment
    "KeyVaultUri"                           = azurerm_key_vault.keyvault.vault_uri
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.app_insights.connection_string
    "ConnectionStrings__DefaultConnection"  = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.keyvault.name};SecretName=ConnectionStrings--DefaultConnection)"
  }
}

resource "azurerm_linux_web_app_slot" "staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.app_service.id
  tags           = local.tags_app_service
  https_only          = true
  
  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false
  
  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on                               = true
    container_registry_use_managed_identity = true
    http2_enabled                           = true
    ftps_state                              = "Disabled"
    health_check_path                       = "/health"
    health_check_eviction_time_in_min       = 5

    application_stack {
      dotnet_version = var.app_service_dotnet_version
    }

    auto_heal_setting {
      action {
        action_type = "Recycle"
      }
      trigger {
        requests {
          count    = 50
          interval = "00:01:00"
        }
      }
    }
  }
  
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.aspnet_core_environment
    "KeyVaultUri"                           = azurerm_key_vault.keyvault.vault_uri
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.app_insights.connection_string
    "ConnectionStrings__DefaultConnection"  = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.keyvault.name};SecretName=ConnectionStrings--DefaultConnection)"
  }
}
