//
// Project: IPNoticeHub
// Purpose: Deploys the App Service and Hosting Plan (lab environment)
// Owner: Nikolay
//

@description('The Azure Subscription ID where the resources will be deployed.')
param subscriptionId string

@description('The name of the Resource Group for the App Service.')
param resourceGroupName string

@description('The name of the App Service (Web App).')
param name string

@description('The Azure Region for all resources (e.g., westeurope).')
param location string

@description('The name of the App Service Plan (Hosting Plan).')
param hostingPlanName string

@description('The Resource Group where the Hosting Plan resides.')
param serverFarmResourceGroup string

@description('If true, keeps the app loaded even when there is no traffic (Requires Basic tier or higher).')
param alwaysOn bool

@description('The state of FTPS encryption. Recommended: FtpsOnly.')
param ftpsState string

@description('The pricing tier of the App Service Plan (e.g., Basic, Standard).')
param sku string

@description('The specific SKU code.')
param skuCode string

@description('The size of the workers.')
param workerSize string

@description('The ID associated with the worker size.')
param workerSizeId string

@description('The number of instances for the App Service Plan.')
param numberOfWorkers string

@description('The runtime stack of the app (e.g., dotnet, php, node).')
param currentStack string

@description('The version of PHP if applicable.')
param phpVersion string

@description('The .NET Framework version if applicable.')
param netFrameworkVersion string

@description('Metadata for the configured stacks in Windows environments.')
param windowsConfiguredStacks array

// --- 1. APP SERVICE PLAN (Hosting Plan) ---
resource hostingPlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: hostingPlanName
  location: location
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'app-service-plan'
  }
  sku: {
    tier: sku
    name: skuCode
  }
  properties: {
    workerSize: workerSize
    workerSizeId: workerSizeId
    numberOfWorkers: int(numberOfWorkers)
    zoneRedundant: false
  }
}

// --- 2. APP SERVICE (Web App) ---
resource name_resource 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'web-app'
  }
  properties: {
    siteConfig: {
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: currentStack
        }
      ]
      phpVersion: phpVersion
      netFrameworkVersion: netFrameworkVersion
      windowsConfiguredStacks: windowsConfiguredStacks
      alwaysOn: alwaysOn
      ftpsState: ftpsState
    }
    serverFarmId: hostingPlan.id
    clientAffinityEnabled: true
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
  }
}

// --- 3. SECURITY POLICIES (Disabling Basic Auth) ---
resource name_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: name_resource
  name: 'scm'
  properties: {
    allow: false
  }
}

resource name_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: name_resource
  name: 'ftp'
  properties: {
    allow: false
  }
}
