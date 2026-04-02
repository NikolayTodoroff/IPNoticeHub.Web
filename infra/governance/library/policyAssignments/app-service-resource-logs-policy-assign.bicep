/*
  App Service Resource Logs Policy Assignment

  Purpose:
  - Ensures that Azure App Services are configured to recreate activity trails for investigation purposes 
    if a security incident occurs or the network is compromised.

  Scope:
  - Subscription
*/

targetScope = 'subscription'
param location string

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

resource appServiceResourceLogsAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location 

  properties: {
    displayName: 'App Service Resource Logs Policy Assignment'
    description: 'Ensures that Azure App Services are configured to recreate activity trails for investigation purposes if a security incident occurs or the network is compromised.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/91a78b24-f231-4a8a-8da9-02c35b2b6510'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
