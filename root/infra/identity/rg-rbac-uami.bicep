/*
  User Assigned Managed Identity RBAC for Log Analytics Workspace Managed Identity

  Purpose:
  - Grants a specified role to a User Assigned Managed Identity at the Resource Group scope.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('The object ID of the Managed Identity.')
param principalId string

@allowed([
  'ServicePrincipal'
  'User'
  'Group'
])
param principalType string = 'ServicePrincipal'

@description('Role definition GUID.')
param roleDefinitionGuid string

@description('A unique key to ensure the GUID name is deterministic.')
param assignmentKey string

var roleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
    name: guid(resourceGroup().id, principalId, roleDefinitionGuid, assignmentKey)
    scope: resourceGroup()
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = ra.id
