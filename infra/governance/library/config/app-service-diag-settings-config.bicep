/*
  App Service Diagnostic Settings Configuration

  Purpose:
  - Creates diagnostic settings for an App Service to send logs and metrics to a specified Log Analytics workspace.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('App Service name to apply the diagnostic settings to.')
param appServiceName string

@description('Unique assignment name at this scope.')
param assignmentName string = 'assign-app-service-diag-iphub-lab-weu'

@description('Resource ID of the Log Analytics workspace that will receive App Service metrics.')
param logAnalyticsWorkspaceResourceId string

@description('Defined list of categories with individual settings')
var logConfigurations = [
  {
    name: 'AppServiceConsoleLogs'
    enabled: true
    retention: 0
  }
  {
    name: 'AppServiceAppLogs'
    enabled: true
    retention: 0
  }
]

resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

resource diagnosticSetting 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: assignmentName
  scope: appService
properties: {
    workspaceId: logAnalyticsWorkspaceResourceId
    logs: [for log in logConfigurations: {
      category: log.name
      enabled: log.enabled
      retentionPolicy: {
        days: log.retention
        enabled: log.retention > 0
      }
    }]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}
