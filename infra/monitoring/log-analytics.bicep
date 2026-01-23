/*
  Log Analytics Workspace

  Purpose:
  Centralized logging store for system metrics and application logs.

  Scope:
  Resource Group
*/

targetScope = 'resourceGroup'

param name string
param location string = resourceGroup().location
param sku string = 'PerGB2018'
param retentionInDays int = 30
param tags object = {}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  tags: union(tags, { purpose: 'analytics' })
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

output workspaceId string = logAnalyticsWorkspace.id
output customerId string = logAnalyticsWorkspace.properties.customerId
