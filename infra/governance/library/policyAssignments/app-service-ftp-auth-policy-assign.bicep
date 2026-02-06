/*
  App Service FTP Auth Policy Assignment

  Purpose:
  - Audits whether App Services have FTP authentication disabled for security.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

resource appServiceFtpAuthAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service FTP Auth Audit Policy Assignment'
    description: 'Audits whether App Services have FTP authentication disabled for security.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/871b205b-57cf-4e1e-a234-492616998bf7'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
