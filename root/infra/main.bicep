var alertsRgName = 'rg-alerts-${workload}-${env}-${region}'

param location string
param alertEmail string

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

resource alertsRgExisting 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  scope: subscription()
  name: alertsRgName
}

module mainRg 'app-platform/rg-ipnoticehub.bicep' = {
  name: 'mainRg'
  scope: subscription()
  params: {
    tags: globalTags
  }
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
    storageAccountName: 'stipnoticehubdocslabweu'
    location: resourceGroup().location
    tags: globalTags
  }
}

module sqlServer 'storage/sql-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: 'sql-ipnoticehub-dev'
    location: resourceGroup().location
    tags: globalTags
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
    name: 'rg-alerts-${workload}-${env}-${region}'
    location: location
    tags: globalTags
  }
}

module logAnalyticsWorkspace 'monitoring/log-analytics.bicep' = {
  name: 'logAnalyticsWorkspace'
  params: {
    name: 'log-ipnoticehub-${env}-${region}'
    location: resourceGroup().location
    tags: globalTags
  }
}

module appInsights 'monitoring/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: 'appi-ipnoticehub-${env}-${region}'
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
    name: 'kv-ipnoticehub-dev-weu'
    location: resourceGroup().location
    tags: globalTags
    publicNetworkAccess: 'Enabled'
    defaultAction: 'Allow'
    bypass: 'AzureServices'
  }
}


