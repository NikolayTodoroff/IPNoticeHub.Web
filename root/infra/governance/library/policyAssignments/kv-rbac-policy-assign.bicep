/*
  Key Vault RBAC Policy Assignment

  Purpose:
  - Audits whether Key Vaults have RBAC enabled for access management.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['Audit','Deny','Disabled'])
param effect string = 'Audit'

resource keyVaultRbacAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Key Vault RBAC Audit Policy Assignment'
    description: 'Audits whether Key Vaults have RBAC enabled for access management.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/12d4fa5e-1f9f-4c21-97a9-b99b3c6611b5'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
