/*
  Virtual Machine Periodic Updates Check Policy Assignment

  Purpose:
  - Ensures that virtual machines are configured to receive updates.

  Scope:
  - Subscription
*/

targetScope = 'subscription'
param location string

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['Audit','Deny','Disabled'])
param effect string = 'Audit'

resource vmUpdatesCheckAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location 

  properties: {
    displayName: 'Virtual Machine Periodic Updates Check Policy Assignment'
    description: 'Ensures that virtual machines are configured to receive updates.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/bd876905-5b84-4f73-ab2d-2e7a7c4568d9'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
