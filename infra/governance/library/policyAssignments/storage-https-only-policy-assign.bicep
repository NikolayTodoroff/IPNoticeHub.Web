/*
  Storage Accounts HTTPS-only Policy Assignment

  Purpose:
  - Enforces HTTPS (secure transfer) for Azure Storage Accounts.
  - Uses built-in Azure Policy definition.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param assignmentName string

@allowed(['Default','DoNotEnforce'])
param enforcementMode string = 'Default'

var policyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/404c3081-a854-4457-ae30-26a93ef643f9'

resource storageHttpsOnlyAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Storage accounts: require secure transfer (HTTPS only)'
    description: 'Enforces secure transfer on storage accounts (HTTPS required).'
    policyDefinitionId: policyDefinitionId
    enforcementMode: enforcementMode

    nonComplianceMessages: [
      {
        message: 'Storage accounts must require secure transfer (HTTPS).'
      }
    ]
  }
}
