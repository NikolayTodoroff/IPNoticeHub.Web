/*
  Key Vault

  Purpose:
  - Central vault for secrets/keys/certs (RBAC mode)

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param name string = 'kv-ipnoticehub-dev-weu'
param location string = resourceGroup().location
param tags object = {}

@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

@allowed([
  'Allow'
  'Deny'
])
param defaultAction string = 'Allow'

@allowed([
  'None'
  'AzureServices'
])
param bypass string = 'AzureServices'

resource keyVault 'Microsoft.KeyVault/vaults@2024-12-01-preview' = {
  name: name
  location: location
  tags: union(tags, { purpose: 'security' })
  properties: {
    tenantId: subscription().tenantId

    sku: {
      family: 'A'
      name: 'standard'
    }

    enableRbacAuthorization: true

    // Network access
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      bypass: bypass
      defaultAction: defaultAction
      ipRules: []
      virtualNetworkRules: []
    }

    // Soft delete & purge protection
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true

    // App integrations
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
  }
}

output kvId string = keyVault.id
output kvName string = keyVault.name
output kvUri string = keyVault.properties.vaultUri

