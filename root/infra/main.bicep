var alertsRgName = 'rg-alerts-${env}-${region}'
var mainRgName = 'rg-ipnoticehub-${env}-${region}'
var networkRgName = 'rg-network-${env}-${region}'
var appInsightsName = 'appi-ipnoticehub-${env}-${region}'
var logAnalyticsName = 'log-ipnoticehub-${env}-${region}'

param location string
param alertEmail string

param keyVaultName string
param sqlDatabaseName string
param sqlServerName string
param storageAccountName string

param sqlMonitoringAssignmentName string
param sqlMonitoringInitiativeName string
param storageAccMonitoringAssignmentName string
param storageAccMonitoringInitiativeName string
param coreSvsMonitoringAssignmentName string
param coreSvsMonitoringInitiativeName string

param policyRemediationUamiResourceId string

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

module mainRg 'app-platform/rg-ipnoticehub.bicep' = {
  name: 'mainRg'
  scope: subscription()
  params: {
    location: location
    rgName: mainRgName
    tags: globalTags
  }
}

module networkRg 'app-platform/rg-network.bicep' = {
  name: 'networkRg'
  scope: subscription()
  params: {
    location: location
    rgName: networkRgName
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

module sqlMonitoring './governance/sql-monitoring-init.bicep' = {
  name: 'sqlMonitoring'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    initiativeName: sqlMonitoringInitiativeName
    assignmentName: sqlMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspace.outputs.workspaceId
  }
}

module appServiceDiagPolicy './governance/app-service-diag-settings-policy.bicep' = {
  name: 'appServiceDiagPolicy'
  scope: subscription()
  params: {
    logAnalytics: logAnalyticsWorkspace.outputs.workspaceId
  }
}

module coreSvsMonitoring './governance/core-services-monitoring-init.bicep' = {
  name: 'coreSvsMonitoring'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    initiativeName: coreSvsMonitoringInitiativeName
    assignmentName: coreSvsMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspace.outputs.workspaceId
    appServicePolicyDefinitionId: appServiceDiagPolicy.outputs.appServicePolicyId
  }
}

module storageAccMonitoring './governance/storage-acc-monitoring-init.bicep' = {
  name: 'storageAccMonitoring'
  scope: subscription()
  params: {
    location: location
    initiativeName: storageAccMonitoringInitiativeName
    assignmentName: storageAccMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspace.outputs.workspaceId
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
  }
}
