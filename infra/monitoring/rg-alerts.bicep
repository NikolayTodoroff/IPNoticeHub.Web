/* SCOPE: Subscription
   PROJECT: IPNoticeHub
   DESCRIPTION: Main Resource Group for application services.
*/

targetScope = 'subscription'

@description('The name of the resource group')
param rgName string = 'rg-ipnoticehub-lab-weu'

@description('The location for the resource group metadata')
param location string = 'westeurope'

// --- RESOURCE DEFINITION ---

resource mainRG 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
  }
}

output rgId string = mainRG.id