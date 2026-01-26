/*
  Storage Account Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures Storage Accounts have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param assignmentName string = 'assign-storage-diag-iphub-lab-weu'

@allowed([
  'DeployIfNotExists'
  'AuditIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings profile name that will be created on each Storage Account.')
param profileName string = 'storageAccountsDiagnosticsLogsToWorkspace'

@description('Resource ID of the Log Analytics workspace that will receive Storage Account metrics.')
param logAnalyticsWorkspaceResourceId string

@description('Whether to enable metrics streaming to Log Analytics for Storage Accounts.')
param metricsEnabled bool = true

resource storageDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy Storage Account diagnostics to Log Analytics Workspace'
    description: 'Ensures Storage Accounts stream AllMetrics to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/59759c62-9a22-4cdf-ae64-074495983fef'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      profileName: { value: profileName }
      logAnalytics: { value: logAnalyticsWorkspaceResourceId }
      metricsEnabled: { value: metricsEnabled }
    }
  }
}
