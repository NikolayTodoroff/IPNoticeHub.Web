// ==============================================================================
// Project: IPNoticeHub
// Purpose: Grant "Key Vault Secrets User" permissions to the Web App Identity.
// Security: Follows Principle of Least Privilege (Read-only access to secrets).
// ==============================================================================

@description('Key Vault name')
param keyVaultName string

@description('Web App managed identity object ID')
param webAppPrincipalId string

resource kv 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource kvSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, webAppPrincipalId, 'kv-secrets-user')
  scope: kv
  properties: {
    roleDefinitionId: '/providers/Microsoft.Authorization/roleDefinitions/4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User
    principalId: webAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}
