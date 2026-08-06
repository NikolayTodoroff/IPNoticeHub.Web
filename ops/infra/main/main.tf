resource "azurerm_resource_group" "rg_main" {
  name     = "rg-main-${local.prefix}"
  location = var.location
}