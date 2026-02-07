/*
  Blobs Versioning Policy Assignment

  Purpose:
  - Enforces versioning to be enabled on Azure Storage Blob containers.

  Scope:
  - Subscription
*/

targetScope = 'subscription'
param location string

@description('Unique policy assignment name at subscription scope.')
param assignmentName string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed(['Modify','Disabled'])
param effect string = 'Modify'

resource blobsVersioningAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location 

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'Blobs Versioning Policy Assignment'
    description: 'Enforces versioning to be enabled on Azure Storage Blob containers.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/978deb5d-c9a7-41f8-b4b2-b76880d0de1f'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect } 
    }
  }
}
