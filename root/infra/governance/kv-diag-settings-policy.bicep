/*
  Key Vault Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures Key Vaults have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param assignmentName string = 'assign-kv-diag-iphub-lab-weu'

@description('Resource ID of the Log Analytics workspace that will receive Key Vault metrics.')
param logAnalyticsWorkspaceResourceId string

@allowed([
  'DeployIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings name that will be created on each Key Vault.')
param diagnosticsSettingNameToUse string = 'KeyVaultDiagnosticsLogsToWorkspace'

@allowed(['True', 'False'])
param auditEventEnabled string = 'True'

@allowed(['True', 'False'])
param allMetricsEnabled string = 'True'

resource kvDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy Key Vault diagnostics to Log Analytics Workspace'
    description: 'Ensures Key Vaults stream AllMetrics and AuditEvent to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/951af2fa-529b-416e-ab6e-066fd85ac459'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      diagnosticsSettingNameToUse: { value: diagnosticsSettingNameToUse }
      logAnalytics: { value: logAnalyticsWorkspaceResourceId }
      AuditEventEnabled: { value: auditEventEnabled }
      AllMetricsEnabled: { value: allMetricsEnabled }
    }
  }
}
