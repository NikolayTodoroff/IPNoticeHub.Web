/*
  Key Vault Public Access Disabled Configuration

  Purpose:
  - Configure Key Vault to disable public network access for enhanced security.

  Scope:
  - Key Vault 
*/

targetScope = 'resourceGroup'

param location string
param keyVaultName string

resource kvLockdown 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId 
    sku: {
      family: 'A'
      name: 'standard'
    }
    
    enableRbacAuthorization: true 
    
    publicNetworkAccess: 'Disabled'
    networkAcls: {
      defaultAction: 'Deny'
      bypass: 'AzureServices'
      ipRules: []
      virtualNetworkRules: []
    }
  }
}

