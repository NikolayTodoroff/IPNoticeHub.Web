/*
  SQL Security Manager RBAC for Resource Group Managed Identity

  Purpose:
  - Grants SQL Security Manager role to a User Assigned Managed Identity at the Resource Group scope.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Principal (object) ID of the managed identity / service principal.')
param principalId string

@allowed([
  'ServicePrincipal'
  'User'
  'Group'
])
param principalType string = 'ServicePrincipal'

@description('Role definition GUID.')
param roleDefinitionGuid string

@description('Stable key used to create deterministic role assignment name.')
param assignmentKey string

var roleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)

resource sqlSecurityManagerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, assignmentKey, roleDefinitionGuid)
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = sqlSecurityManagerRoleAssignment.id
