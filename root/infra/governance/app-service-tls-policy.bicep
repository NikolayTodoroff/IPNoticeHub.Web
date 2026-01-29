/*
  App Service Latest TLS Version Policy Assignment

  Purpose:
  - Assigns a policy that ensures App Services enforce latest TLS version.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param assignmentName string

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

resource AppServiceTlsAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service: Enforce latest TLS version'
    description: 'Ensures App Services enforce the latest TLS version.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/f0e6e85b-9b9f-4a4b-b67b-f730d42f1b0b'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
    }
  }
}
