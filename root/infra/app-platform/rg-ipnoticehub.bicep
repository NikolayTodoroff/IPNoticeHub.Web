/*
  Main App Resource Group

  Purpose:
  - Main resource group for application services.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param rgName string
param location string
param tags object = {}

resource mainRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: union(tags, { purpose: 'rg' })
}

output rgId string = mainRG.id
output rgNameOut string = mainRG.name
output rgLocation string = mainRG.location
