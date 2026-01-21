//
// Project: IPNoticeHub
// Purpose: Enable System-Assigned Managed Identity for the Web App
// 

@description('Name of the existing Web App')
param webAppName string

@description('Location of the Web App')
param location string = 'westeurope'

@description('Resource group where the Web App exists')
param resourceGroupName string

// --- WEB APP RESOURCE UPDATE ---

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

// --- OUTPUTS ---

@description('The Principal ID of the newly created Managed Identity. Use this for RBAC assignments.')
output webAppMSIPrincipalId string = webApp.identity.principalId
