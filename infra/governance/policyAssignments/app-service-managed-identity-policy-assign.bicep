/*
  App Service Managed Identity Policy Assignment

  Purpose:
  - Audits whether App Services have a managed identity assigned for secure access management.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

resource appServiceManagedIdentityAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service Managed Identity Audit Policy Assignment'
    description: 'Audits whether App Services have a managed identity assigned.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/2b9ad585-36bc-4615-b300-fd4435808332'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
