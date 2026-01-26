/*
  Blob Service Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures Blob Services have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param assignmentName string = 'assign-blob-services-diag-iphub-lab-weu'

@allowed([
  'DeployIfNotExists'
  'AuditIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings profile name that will be created on each Blob Service.')
param profileName string = 'blobServicesDiagnosticsLogsToWorkspace'

@description('Resource ID of the Log Analytics workspace that will receive Blob Service metrics.')
param logAnalyticsWorkspaceResourceId string

@description('Whether to enable metrics streaming to Log Analytics for Blob Services.')
param metricsEnabled bool = true

@description('Whether to enable logs streaming to Log Analytics for Blob Services.')
param logsEnabled bool = true

resource blobServDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy Blob Service diagnostics to Log Analytics Workspace'
    description: 'Ensures Blob Services stream AllMetrics to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/b4fe1a3b-0715-4c6c-a5ea-ffc33cf823cb'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      profileName: { value: profileName }
      logAnalytics: { value: logAnalyticsWorkspaceResourceId }
      metricsEnabled: { value: metricsEnabled }
      logsEnabled: { value: logsEnabled }
  }
}
}
