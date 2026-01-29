/*
  Main Network Resource Group

  Purpose:
  - Main resource group for network resources.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param rgName string
param location string
param tags object = {}

resource networkRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: union(tags, { purpose: 'rg' })
}

output rgId string = networkRG.id
output rgNameOut string = networkRG.name
output rgLocation string = networkRG.location
