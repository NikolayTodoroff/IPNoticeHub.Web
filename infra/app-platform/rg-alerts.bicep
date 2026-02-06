/*
  Alerts Resource Group

  Purpose:
  - Alerts resource group for alerting resources.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param name string
param location string
param tags object = {}

resource alertsRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: name
  location: location
  tags: union(tags, { purpose: 'rg' })
}

output rgId string = alertsRG.id
output rgNameOut string = alertsRG.name
output rgLocation string = alertsRG.location
