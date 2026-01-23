param env string = 'lab'
param owner string = 'nikolay'
param regionTag string = 'weu'
param workload string = 'ipnoticehub'

var alertsRgName = 'rg-alerts-${workload}-${env}-${regionTag}'

resource alertsRgExisting 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  scope: subscription()
  name: alertsRgName
}

var globalTags = {
  env: env
  owner: owner
  region: regionTag
  workload: workload
}

module identityRegistry 'identity/identity-registry.bicep' = {
  name: 'identityRegistry'
  scope: tenant()
}

module sqlServer 'storage/sql-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: 'ipnoticehub-sql'
    location: resourceGroup().location
    tags: globalTags
  }
}

module sqlAdmin 'identity/sql-admin.bicep' = {
  name: 'sqlAdmin'
  params: {
    serverName: sqlServer.outputs.serverName
    entraAdminLogin: identityRegistry.outputs.globalAdminUpn
    entraAdminObjectId: identityRegistry.outputs.globalAdminObjectId
  }
}

module sqlDatabase 'storage/sql-db.bicep' = {
  name: 'sqlDatabase'
  params: {
    serverName: sqlServer.outputs.serverName
    databaseName: 'ipnoticehub-db'
    location: resourceGroup().location
    tags: globalTags
  }
}

module alertsRg 'monitoring/rg-alerts.bicep' = {
  name: 'alertsRg'
  scope: subscription()
  params: {
    name: 'rg-alerts-${workload}-${env}-${regionTag}'
    location: 'westeurope'
    tags: globalTags
  }
}

module logAnalyticsWorkspace 'monitoring/log-analytics.bicep' = {
  name: 'logAnalyticsWorkspace'
  params: {
    name: 'log-ipnoticehub-${env}-${regionTag}'
    location: resourceGroup().location
    tags: globalTags
  }
}

module appInsights 'monitoring/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: 'appi-ipnoticehub-${env}-${regionTag}'
    location: resourceGroup().location
    workspaceResourceId: logAnalyticsWorkspace.outputs.workspaceId
    tags: globalTags
  }
}

module budget 'monitoring/budget.bicep' = {
  name: 'budget'
  scope: subscription()
  params: {
    budgetName: 'bud-iphub-sub-lab'
    budgetAmount: 100
    startDate: '2026-01-01T00:00:00Z'
    endDate: '2027-12-31T00:00:00Z'
    contactEmails: [ 'everflowing555@gmail.com' ]

    agInfoId: actionGroups.outputs.actionGroupIds.info
    agWarnId: actionGroups.outputs.actionGroupIds.warn
    agCritId: actionGroups.outputs.actionGroupIds.crit
  }
}

module actionGroups 'monitoring/action-groups.bicep' = {
  name: 'actionGroups'
  scope: alertsRgExisting
  params: {
    alertEmail: 'everflowing555@gmail.com'
    tags: globalTags
  }
  dependsOn: [
    alertsRg
  ]
}

module webApp 'app-platform/app-service.bicep' = {
  name: 'webApp'
  params: {
    appName: 'ipnoticehub-web-lab'
    hostingPlanName: 'asp-iphub-lab-weu'
    location: resourceGroup().location
    tags: globalTags

    skuName: 'B1'
    skuTier: 'Basic'
    capacity: 1

    alwaysOn: true
    ftpsState: 'Disabled'

    siteConfigOverrides: {
      windowsFxVersion: 'DOTNET|8.0'
      minTlsVersion: '1.2'
      http20Enabled: true
    }
  }
}

module webAppIdentity 'identity/webapp.identity.bicep' = {
  name: 'webAppIdentity'
  params: {
    webAppName: webApp.outputs.webAppName
    location: resourceGroup().location
  }
}

module storageAcc 'app-platform/storage-acc.bicep' = {
  name: 'storageAcc'
  params: {
    storageAccountName: 'stipnoticehubdocslabweu'
    location: resourceGroup().location
    tags: globalTags
  }
}

module mainRg 'app-platform/rg-ipnoticehub.bicep' = {
  name: 'mainRg'
  scope: subscription()
  params: {
    tags: globalTags
  }
}

module keyVault 'governance/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    name: 'kv-ipnoticehub-dev-weu'
    location: resourceGroup().location
    tags: globalTags
    publicNetworkAccess: 'Enabled'
    defaultAction: 'Allow'
    bypass: 'AzureServices'
  }
}

module kvRbacSecretUser 'governance/key-vault.rbac.bicep' = {
  name: 'kvRbacSecretUser'
  params: {
    keyVaultName: keyVault.outputs.kvName
    principalId: webAppIdentity.outputs.webAppPrincipalId
    principalType: 'ServicePrincipal'
    roleDefinitionGuid: '4633458b-17de-408a-b874-0445c86b69e6'
    assignmentKey: 'kv-secrets-user'
  }
}

module kvRbacSecretOfficer 'governance/key-vault.rbac.bicep' = {
  name: 'kvRbacSecretOfficer'
  params: {
    keyVaultName: keyVault.outputs.kvName
    principalId: identityRegistry.outputs.globalAdminObjectId
    principalType: 'User'
    roleDefinitionGuid: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
    assignmentKey: 'kv-secrets-officer'
  }
}
