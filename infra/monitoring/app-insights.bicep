/*
  Application Insights Resource

  Purpose:
   - Provides application performance monitoring (APM) and telemetry for web applications
   - Enables real-time diagnostics, performance tracking, and usage analytics
   - Workspace-based configuration linked to Log Analytics

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param workspaceResourceId string
param tags object = {}

@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccessForIngestion string = 'Enabled'

@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccessForQuery string = 'Enabled'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceResourceId

    publicNetworkAccessForIngestion: publicNetworkAccessForIngestion
    publicNetworkAccessForQuery: publicNetworkAccessForQuery
  }
}

output appInsightsId string = appInsights.id
output instrumentationKey string = appInsights.properties.InstrumentationKey
output connectionString string = appInsights.properties.ConnectionString

