/*
  SQL Database TLS Audit Policy Assignment

  Purpose:
  - Audits whether TLS encryption is enabled for Azure SQL Databases.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@allowed(['Audit','Deny','Disabled'])
param effect string = 'Audit'

resource sqlDbTlsAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'SQL Database TLS Audit Policy Assignment'
    description: 'Audits whether TLS encryption is enabled for Azure SQL Databases.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/32e6bbec-16b6-44c2-be37-c5b672d103cf'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
