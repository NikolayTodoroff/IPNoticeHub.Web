/*
  User Assigned Managed Identity for Policy Remediation

  Purpose:
  - Creates a User Assigned Managed Identity (UAMI) to be used for Azure Policy remediation tasks that require identity.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Name of the User Assigned Managed Identity used for Azure Policy remediation.')
param name string

@description('Location for the identity (usually same region as your core resources).')
param location string

resource policyUami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
}

output uamiResourceId string = policyUami.id
output principalId string = policyUami.properties.principalId
output clientId string = policyUami.properties.clientId
