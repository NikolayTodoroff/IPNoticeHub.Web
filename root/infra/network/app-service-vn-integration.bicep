/*
  App Service Virtual Network Integration

  Purpose:
  - Creates an App Service with Virtual Network Integration
  - Integrates the App Service with a subnet in a Virtual Network

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Name of the existing App Service (Web App).')
param appServiceName string

@description('Subnet resource ID for Regional VNet Integration (must be delegated to Microsoft.Web/serverFarms).')
param subnetId string

@description('Location for the App Service.')
param location string = resourceGroup().location

@description('Enable "Route All" so all outbound traffic goes through the VNet.')
param enableRouteAll bool = false

// App Service with VNet Integration 
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  properties: {
    virtualNetworkSubnetId: subnetId
  }
}

// Route all outbound traffic through the VNet if specified
resource webConfig 'Microsoft.Web/sites/config@2022-09-01' = if (enableRouteAll) {
  parent: webApp
  name: 'web'
  properties: {
    vnetRouteAllEnabled: true
  }
}

output configuredSubnetId string = webApp.properties.virtualNetworkSubnetId

