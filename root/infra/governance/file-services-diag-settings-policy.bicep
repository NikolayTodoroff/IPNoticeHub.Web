/*
  File Service Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures File Services have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param assignmentName string = 'assign-file-services-diag-iphub-lab-weu'

@allowed([
  'DeployIfNotExists'
  'AuditIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings profile name that will be created on each File Service.')
param profileName string = 'fileServicesDiagnosticsLogsToWorkspace'

@description('Resource ID of the Log Analytics workspace that will receive File Service metrics.')
param logAnalytics string

@description('Whether to enable metrics streaming to Log Analytics for File Services.')
param metricsEnabled bool = true

@description('Whether to enable logs streaming to Log Analytics for File Services.')
param logsEnabled bool = true

resource fileServDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy File Service diagnostics to Log Analytics Workspace'
    description: 'Ensures File Services stream AllMetrics to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/25a70cc8-2bd4-47f1-90b6-1478e4662c96'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      profileName: { value: profileName }
      logAnalytics: { value: logAnalytics }
      metricsEnabled: { value: metricsEnabled }
      logsEnabled: { value: logsEnabled }
  }
}
}
