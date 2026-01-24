targetScope = 'resourceGroup'

var keyVaultSecretsUserRole = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultSecretsOfficerRole = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

param keyVaultName string
param globalAdminObjectId string
param sqlServerName string
param webAppName string

module identityRegistry './identity-registry.bicep' = {
  name: 'identityRegistry'
  scope: tenant()
}

module securityGroupsRegistry './security-groups-registry.bicep' = {
  name: 'securityGroupsRegistry'
  scope: tenant()
}

module webAppIdentity './webapp.identity.bicep' = {
  name: 'webAppIdentity'
  params: {
    webAppName: webAppName
    location: resourceGroup().location
  }
}

module kvRbacSecretUser './key-vault.rbac.bicep' = {
  name: 'kvRbacSecretUser'
  params: {
    keyVaultName: keyVaultName
    principalId: webAppIdentity.outputs.webAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: keyVaultSecretsUserRole
    assignmentKey: 'kv-secrets-user'
  }
}

module kvRbacSecretOfficer './key-vault.rbac.bicep' = {
  name: 'kvRbacSecretOfficer'
  params: {
    keyVaultName: keyVaultName
    principalId: globalAdminObjectId
    principalType: 'User'
    roleDefinitionGuid: keyVaultSecretsOfficerRole
    assignmentKey: 'kv-secrets-officer'
  }
}

module sqlAdmin './sql-admin.bicep' = {
  name: 'sqlAdmin'
  params: {
    serverName: sqlServerName
    entraAdminLogin: identityRegistry.outputs.globalAdminUpn
    entraAdminObjectId: identityRegistry.outputs.globalAdminObjectId
  }
}

