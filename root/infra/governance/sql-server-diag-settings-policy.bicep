/*
  SQL Servers Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures SQL Servers have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string = 'assign-sqlserver-diag-iphub-lab-weu'

@description('Resource ID of the Log Analytics workspace that will receive SQL Server logs/metrics.')
param logAnalyticsWorkspaceId string

@allowed([
  'DeployIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

resource sqlServerDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy SQL Server diagnostic settings to Log Analytics (IPHub)'
    description: 'Ensures SQL Servers stream selected diagnostic logs and metrics to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/7ea8a143-05e3-4553-abfe-f56bef8b0b70'
    enforcementMode: 'Default'
    parameters: {
      logAnalyticsWorkspaceId: { value: logAnalyticsWorkspaceId }
      effect: { value: effect } 
    }
  }
}
