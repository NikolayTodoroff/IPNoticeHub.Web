/*
  SQL Server Contributor RBAC for Resource Group Managed Identity

  Purpose:
  - Grants SQL Server Contributor role to a User Assigned Managed Identity at the Resource Group scope.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Principal (object) ID of the managed identity / service principal.')
param principalId string

@allowed(['ServicePrincipal','User','Group'])
param principalType string = 'ServicePrincipal'

@description('SQL Server Contributor Role definition GUID.')
param roleDefinitionGuid string

@description('Stable key used to create deterministic role assignment name.')
param assignmentName string

var roleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, assignmentName, roleDefinitionGuid)
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = ra.id
