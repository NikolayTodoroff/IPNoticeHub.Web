/*
  Restrict Access to App Service Advanced Tool Site (Kudu) - Configuration

  Purpose:
  - Allow personal public IP addresses to access Kudu site for App Services with Priority 100.
  - Deny all other public access to Kudu site for App Services with Priority 200.

  Scope:
  - App Service 
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

