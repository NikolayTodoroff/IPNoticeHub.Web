/*
  Web App - Enable System Assigned Managed Identity (SMI)

  Purpose:
  - Enables SystemAssigned identity on an existing Web App
  - Outputs principalId for downstream RBAC (Key Vault, etc.)

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param webAppName string
param location string

resource webAppIdentity 'Microsoft.Web/sites@2024-11-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
}

output webAppPrincipalId string = webAppIdentity.identity.principalId
output webAppResourceId string = webAppIdentity.id
