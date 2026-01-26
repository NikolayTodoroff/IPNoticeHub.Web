/*
  Log Analytics Contributor RBAC for Log Analytics Workspace Managed Identity

  Purpose:
  - Grants Log Analytics Contributor role to a User Assigned Managed Identity at Log Analytics Workspace scope.

  Scope:
  - Log Analytics Workspace
*/

targetScope = 'resourceGroup'

@description('Name of the Log Analytics workspace.')
param logAnalyticsWorkspaceName string

@description('Principal (object) ID of the managed identity / service principal.')
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

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-08-01' existing = {
  name: logAnalyticsWorkspaceName
}

var roleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(logAnalyticsWorkspace.id, principalId, assignmentKey, roleDefinitionGuid)
  scope:logAnalyticsWorkspace
  properties: {
    roleDefinitionId: roleDefinitionId
    principalId: principalId
    principalType: principalType
  }
}

output assignmentId string = ra.id
