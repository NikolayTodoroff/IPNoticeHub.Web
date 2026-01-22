param env string = 'lab'
param owner string = 'nikolay'
param regionTag string = 'weu'
param workload string = 'iphub'

var alertsRgName = 'rg-alerts-${workload}-${env}-${regionTag}'

resource alertsRgExisting 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  scope: subscription()
  name: alertsRgName
}

var commonTags = {
  env: env
  owner: owner
  region: regionTag
  workload: workload
}

module sqlServer 'storage/sql-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: 'ipnoticehub-sql' 
    location: resourceGroup().location
    tags: union(commonTags, { purpose: 'db' })
    entraAdminName: 'nikolay.todorov@ipnoticehub.com'
    entraAdminObjectId: '805ed398-18bb-4b1d-bfa7-bdd99bea4e4'
  }
}

module sqlDatabase 'storage/sql-db.bicep' = {
  name: 'sqlDatabase'
  params: {
    serverName: 'ipnoticehub-sql'
    databaseName: 'ipnoticehub-db'
    location: resourceGroup().location
    tags: union(commonTags, { purpose: 'db' })
  }
}

module alertsRg 'monitoring/rg-alerts.bicep' = {
  name: 'alertsRg'
  scope: subscription()
  params: {
    name: 'rg-alerts-${workload}-${env}-${regionTag}'
    location: 'westeurope'
    tags: union(commonTags, { purpose: 'alerts' })
  }
}

module logAnalyticsWorkspace 'monitoring/log-analytics.bicep' = {
  name: 'logAnalyticsWorkspace'
  params: {
    name: 'log-ipnoticehub-${env}-${regionTag}'
    location: resourceGroup().location
    tags: union(commonTags, { purpose: 'analytics' })
  }
}

module appInsights 'monitoring/app-insights.bicep' = {
  name: 'appInsights'
  params: {
    name: 'appi-ipnoticehub-${env}-${regionTag}'
    location: resourceGroup().location
    workspaceResourceId: logAnalyticsWorkspace.outputs.workspaceId
    tags: union(commonTags, { purpose: 'analytics' })
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
    tags: union(commonTags, { purpose: 'alerts' })
  }
  dependsOn: [
    alertsRg
  ]
}

