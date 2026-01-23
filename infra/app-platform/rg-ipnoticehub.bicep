/*
  Main App Resource Group

  Purpose:
  - Main resource group for application services.

  Scope:
  - Subscription (IPHub-Portfolio-Sub)
*/

targetScope = 'subscription'

param rgName string = 'rg-ipnoticehub-lab-weu'
param location string = 'westeurope'
param tags object = {}

resource mainRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: union(tags, { purpose: 'rg' })
}

output rgId string = mainRG.id
output rgNameOut string = mainRG.name
output rgLocation string = mainRG.location
