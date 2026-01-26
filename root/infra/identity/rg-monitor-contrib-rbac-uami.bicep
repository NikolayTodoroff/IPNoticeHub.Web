/*
  Monitoring Contributor RBAC for Resource Group Managed Identity

  Purpose:
  - Grants Monitoring Contributor role to a User Assigned Managed Identity at the Resource Group scope.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Principal (object) ID of the identity / user / SPN.')
param principalId string

@allowed([
  'ServicePrincipal'
  'User'
  'Group'
])
param principalType string = 'ServicePrincipal'

@description('Role definition GUID (not full resource ID).')
param roleDefinitionGuid string

@description('Stable key used to create deterministic role assignment name.')
param assignmentKey string

@description('Scope resource ID to bind the role assignment to. Defaults to the resource group.')
param scopeResourceId string = resourceGroup().id

var roleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(scopeResourceId, principalId, assignmentKey, roleDefinitionGuid)
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = ra.id
