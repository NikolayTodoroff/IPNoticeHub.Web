/*
  App Service (Web App) + App Service Plan

  Purpose:
  - Creates an App Service Plan and a Windows Web App
  - Applies baseline security settings (HTTPS only, disables basic publishing creds for FTP/SCM)
  - Allows runtime/siteConfig customization via siteConfigOverrides

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param appName string
param hostingPlanName string
param location string = resourceGroup().location
param tags object = {}

param skuName string
param skuTier string
param capacity int = 1
param alwaysOn bool = true

@allowed([
  'AllAllowed'
  'Disabled'
  'FtpsOnly'
])
param ftpsState string = 'Disabled'

param systemAssignedIdentityEnabled bool = true
param currentStack string = 'dotnet'
param siteConfigOverrides object = {}

// App Service Plan
resource hostingPlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: hostingPlanName
  location: location
  tags: union(tags, { purpose: 'web' })

  sku: {
    name: skuName
    tier: skuTier
    capacity: capacity
  }
  
  properties: {
    zoneRedundant: false
  }
}

// Web App
var baseSiteConfig = {
  alwaysOn: alwaysOn
  ftpsState: ftpsState

  metadata: [
    {
      name: 'CURRENT_STACK'
      value: currentStack
    }
  ]
}

resource webApp 'Microsoft.Web/sites@2024-11-01' = {
  name: appName
  location: location
  tags: tags

  identity: systemAssignedIdentityEnabled ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    clientAffinityEnabled: true
    publicNetworkAccess: 'Enabled'
    siteConfig: union(baseSiteConfig, siteConfigOverrides)
  }
}

// Security Enhancements: Disabled Basic Publishing Credentials for FTP and SCM
resource basicCredsScm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: webApp
  name: 'scm'
  properties: {
    allow: false
  }
}

resource basicCredsFtp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: webApp
  name: 'ftp'
  properties: {
    allow: false
  }
}

output webAppName string = webApp.name
output webAppId string = webApp.id
output hostingPlanId string = hostingPlan.id
output principalId string = systemAssignedIdentityEnabled ? webApp.identity.principalId : ''
