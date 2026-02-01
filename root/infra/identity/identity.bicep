targetScope = 'resourceGroup'

var keyVaultSecretsUserRole = '4633458b-17de-408a-b874-0445c86b69e6'
var keyVaultSecretsOfficerRole = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
var monitoringContributorRole = '749f88d5-cbae-40b8-bcfc-e573ddc772fa'
var sqlSecurityManagerRole = '056cd41c-7e88-42e1-933e-88ba6a50c9c3'
var logAnalyticsContributorRole = '92aaf0da-9dab-42b6-94a3-d43ce8d16293'
var storageAccountContributorRole = '17d1049b-9a84-46fb-8f53-869881c3d3ab'
var sqlServerContributorRole = '6d8ee4ec-f05a-4a1d-8b00-a9b17e38b437'

param keyVaultName string
param globalAdminObjectId string
param sqlServerName string
param webAppName string
param uamiPolRemediationName string
param logAnalyticsWorkspaceName string

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

module uamiPolRemediation './policy-remediation-uami.bicep' = {
  name: 'uamiPolRemediation'
  params: {
    name: uamiPolRemediationName  
    location: resourceGroup().location
  }
}

module uamiRgMonitoringContributor './rg-rbac-uami.bicep' = {
  name: 'uami-rg-monitoring-contributor'
  params: {
    principalId: uamiPolRemediation.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: monitoringContributorRole
    assignmentKey: 'monitoring-contrib-rbac-uami-assign'
  }
}

module uamiRgSqlSecurityManager './rg-rbac-uami.bicep' = {
  name: 'uami-rg-sql-security-manager'
  params: {
    principalId: uamiPolRemediation.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: sqlSecurityManagerRole
    assignmentKey: 'sql-security-manager-rbac-uami-assign'
  }
}

module uamiRgSqlServerContributor './rg-sql-server-contr-rbac-uami.bicep' = {
  name: 'uami-rg-sql-server-contributor'
  params: {
    principalId: uamiPolRemediation.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: sqlServerContributorRole
    assignmentKey: 'sql-server-contributor-rbac-uami-assign'
  }
}

module uamiRgStorageAccountContributor './rg-rbac-uami.bicep' = {
  name: 'uami-rg-storage-account-contributor'
  params: {
    principalId: uamiPolRemediation.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: storageAccountContributorRole
    assignmentKey: 'storage-account-contributor-rbac-uami-assign'
  }
}

module uamiWorkspaceLogAnalyticsContributor './log-analytics-contrib-rbac-uami.bicep' = {
  name: 'uami-workspace-loganalytics-contributor'
  params: {
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    principalId: uamiPolRemediation.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: logAnalyticsContributorRole
    assignmentKey: 'log-analytics-contrib-rbac-uami-assign'
  }
}

