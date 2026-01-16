/* SCOPE: Resource Group
   PROJECT: IPNoticeHub
   DESCRIPTION: Centralized logging store for system metrics and application logs.
*/

@description('The name of the Log Analytics workspace')
param name string = 'log-ipnoticehub-lab-weu'

@description('The location for the workspace')
param location string = resourceGroup().location

@description('The pricing tier of the workspace')
@allowed([
  'PerGB2018'
  'Free'
  'Standalone'
])
param sku string = 'PerGB2018'

@description('The resource tags')
param tags object

// --- LOG ANALYTICS WORKSPACE DEFINITION ---
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: sku
    }
  
    retentionInDays: 30
    features: {

      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

output workspaceId string = logAnalyticsWorkspace.id
output customerId string = logAnalyticsWorkspace.properties.customerId
