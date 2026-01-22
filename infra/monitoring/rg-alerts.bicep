/*
  Budget Alert Resource Group

  Purpose:
  - Centralized resource group for hosting monitoring and alerting infrastructure components
  - Contains alert rules, action groups, and notification configurations
  - Enables consistent alerting across the IPHub Portfolio subscription

  Scope:
  - Subscription (IPHub-Portfolio-Sub)
*/

targetScope = 'subscription'

param name string
param location string
param tags object = {}

resource mainRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: name
  location: location
  tags: tags
}

output rgId string = mainRG.id
