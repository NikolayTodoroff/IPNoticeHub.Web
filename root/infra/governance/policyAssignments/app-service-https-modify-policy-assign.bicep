/*
  App Service HTTPS Enforcement Policy Assignment

  Purpose:
  - Enforces HTTPS (secure transfer) for Azure App Services.

  Scope:
  - Subscription
*/

targetScope = 'subscription'
param location string

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed(['Modify','Disabled'])
param effect string = 'Modify'

resource appServiceHttpsAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location 

identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'App Service HTTPS Enforcement Policy Assignment'
    description: 'Enforces HTTPS (secure transfer) for Azure App Services.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/0f98368e-36bc-4716-8ac2-8f8067203b63'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
