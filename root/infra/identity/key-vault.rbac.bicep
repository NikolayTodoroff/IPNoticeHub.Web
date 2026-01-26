/*
  Key Vault RBAC Role Assignments (Secret User,Group,ServicePrincipal)

  Purpose:
  - Grant access to read and write secrets in Key Vault to designated identities.
  - Follows Principle of Least Privilege (Read and write access to secrets).

  Scope:
  - Key Vault 
*/

targetScope = 'resourceGroup'

param keyVaultName string
param principalId string

@allowed([
  'User'
  'Group'
  'ServicePrincipal'
])
param principalType string

param roleDefinitionGuid string
param assignmentKey string

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

var roleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  roleDefinitionGuid
)

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, assignmentKey)
  scope: keyVault
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = ra.id

