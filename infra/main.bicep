var alertsRgName = 'rg-alerts-${env}-${region}'
var mainRgName = 'rg-ipnoticehub-${env}-${region}'
var networkRgName = 'rg-network-${env}-${region}'
var appInsightsName = 'appi-ipnoticehub-${env}-${region}'
var logAnalyticsName = 'log-ipnoticehub-${env}-${region}'
var uamiPolRemediationName= 'uami-iphub-policy-remediation-${env}-${region}'

param location string
param alertEmail string

param policyRemediationUamiResourceId string
param globalAdminObjectId string
param adminSecurityGroupName string
param userSecurityGroupName string

param breakGlassUpn string
param globalAdminUpn string
param sqlAdminUpn string
param testUserUpn string

param dbPrivateDnsName string
param kvPrivateDnsName string
param blobPrivateDnsName string

param env string 
param owner string
param region string
param workload string

var globalTags = {
  env: env
  owner: owner
  region: region
  workload: workload
}

param keyVaultName string
param sqlDatabaseName string
param sqlServerName string
param storageAccountName string
param appServiceName string

module identity './identity/identity.bicep' = {
  name: 'deploy-identity'
  params: {
    keyVaultName: keyVaultName
    webAppName: appServiceName
    sqlServerName: sqlServerName
    uamiPolRemediationName: uamiPolRemediationName
    logAnalyticsWorkspaceName: logAnalyticsName
    globalAdminObjectId: globalAdminObjectId

    breakGlassUpn: breakGlassUpn
    globalAdminUpn: globalAdminUpn
    sqlAdminUpn: sqlAdminUpn
    testUserUpn: testUserUpn
  }
}

module governance './governance/governance.bicep' = {
  name: 'deploy-governance'
  params: {
    contactEmail: alertEmail
    location: location
    keyVaultName: keyVaultName
    logAnalyticsWorkspaceId: logAnalyticsWorkspace.outputs.workspaceId
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    alertsRgName: alertsRgName
    mainRgName: mainRgName
    appServiceName: appServiceName
    storageAccountName: storageAccountName

    workload: workload
    env: env
    region: region
    owner: owner
  }
}

module network './network/network.bicep' = {
  name: 'deploy-network'
  params: {
    location: location
    sqlServerResourceId: sqlServer.outputs.serverId
    keyVaultResourceId: keyVault.outputs.keyVaultId
    storageAccountResourceId: storageAcc.outputs.storageAccountId
    appServiceName: appServiceName
    appServicePlanName: webApp.outputs.appServicePlanName
    dbPrivateDnsName: dbPrivateDnsName
    kvPrivateDnsName: kvPrivateDnsName
    blobPrivateDnsName: blobPrivateDnsName


    workload: workload
    env: env
    region: region
    owner: owner
  }
}

module mainRg 'app-platform/rg-ipnoticehub.bicep' = {
  name: 'mainRg'
  scope: subscription()
  params: {
    location: location
    name: mainRgName
    tags: globalTags
  }
}

module networkRg 'app-platform/rg-network.bicep' = {
  name: 'networkRg'
  scope: subscription()
  params: {
    location: location
    name: networkRgName
    tags: globalTags
  }
}

module alertsRg 'app-platform/rg-alerts.bicep' = {
  name: 'alertsRg'
  scope: subscription()
  params: {
    name: alertsRgName
    location: location
    tags: globalTags
  }
}

resource alertsRgExisting 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  scope: subscription()
  name: alertsRgName
}

module securityGroupsRegistry './app-platform/security-groups-registry.bicep' = {
  name: 'securityGroupsRegistry'
  scope: tenant()
  params: {
    adminSecurityGroupName: adminSecurityGroupName
    userSecurityGroupName: userSecurityGroupName
  }
}

module webApp 'app-platform/app-service.bicep' = {
  name: 'webApp'
  params: {
    appName: appServiceName
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

module storageAcc 'app-platform/storage-acc.bicep' = {
  name: 'storageAcc'
  params: {
    storageAccountName: storageAccountName
    location: resourceGroup().location
    tags: globalTags
  }
}

module sqlServer 'storage/sql-db-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: sqlServerName
    location: resourceGroup().location
    tags: globalTags
  }
}

module sqlDatabase 'storage/sql-db.bicep' = {
  name: 'sqlDatabase'
  params: {
    serverName: sqlServer.outputs.serverName
    databaseName: sqlDatabaseName
    location: resourceGroup().location
    tags: globalTags
  }
}

module logAnalyticsWorkspace 'monitoring/log-analytics.bicep' = {
  name: 'logAnalyticsWorkspace'
  params: {
    name: logAnalyticsName
    location: resourceGroup().location
    tags: globalTags
  }
}

module appInsights 'monitoring/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: appInsightsName
    location: resourceGroup().location
    workspaceResourceId: logAnalyticsWorkspace.outputs.workspaceId
    tags: globalTags
  }
}

module actionGroups 'monitoring/action-groups.bicep' = {
  name: 'actionGroups'
  scope: alertsRgExisting
  params: {
    alertEmail: alertEmail
    tags: globalTags
  }
  dependsOn: [
    alertsRg
  ]
}

module keyVault 'app-platform/key-vault.bicep' = {
  name: 'keyVault'
  params: {
    name: keyVaultName
    location: resourceGroup().location
    tags: globalTags
    publicNetworkAccess: 'Enabled'
    defaultAction: 'Allow'
    bypass: 'AzureServices'
  }
}

module sqlDbBackupRetention './storage/sql-db-backup-retention.bicep' = {
  name: 'sqlDbBackupRetention'
  params: {
    sqlServerName: sqlServer.outputs.serverName
    sqlDatabaseName: sqlDatabase.outputs.databaseName
  }
}

module rgNetworkDeleteLock './app-platform/rg-delete-lock.bicep' = {
  name: 'rgNetworkDeleteLock'
  params: {
    lockLevel: 'CanNotDelete'
    lockName: 'lock-rg-cannotdelete-network'
  }
}

module kvDeleteLock './app-platform/kv-delete-lock.bicep' = {
  name: 'kvDeleteLock'
  params: {
    lockLevel: 'CanNotDelete'
    lockName: 'lock-kv-cannotdelete'
    keyVaultName: keyVaultName
  }
}

module sqlServerDeleteLock './app-platform/sql-server-delete-lock.bicep' = {
  name: 'sqlServerDeleteLock'
  params: {
    lockLevel: 'CanNotDelete'
    lockName: 'lock-sqlserver-cannotdelete'
    sqlServerName: sqlServerName
  }
}
