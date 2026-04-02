/*
  Bastion Log Analytics Policy Assignment

  Purpose:
  - Ensures that Azure Bastion is configured to send logs to Log Analytics.

  Scope:
  - Subscription
*/

targetScope = 'subscription'
param location string

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed(['DeployIfNotExists','AuditIfNotExists','Disabled'])
param effect string = 'DeployIfNotExists'

resource bastionLogAnalyticsAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location 

identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'Bastion Log Analytics Policy Assignment'
    description: 'Ensures that Azure Bastion is configured to send logs to Log Analytics.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/f8352124-56fa-4f94-9441-425109cdc14b'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
