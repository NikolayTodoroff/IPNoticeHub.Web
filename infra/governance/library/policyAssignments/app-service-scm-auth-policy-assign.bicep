/*
  App Service SCM Auth Policy Assignment

  Purpose:
  - Audits whether App Services have SCM authentication disabled for security.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

resource appServiceScmAuthAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service SCM Auth Audit Policy Assignment'
    description: 'Audits whether App Services have SCM authentication disabled for security.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/aede300b-d67f-480a-ae26-4b3dfb1a1fdc'
    enforcementMode: 'Default'
    parameters: {
      effect: {
        type: 'String'
        allowedValues: ['Deny','Audit','Disabled']
        defaultValue: effect
      } 
    }
  }
}
