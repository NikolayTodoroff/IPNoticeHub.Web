/*
  Key Vault Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures Key Vaults have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique assignment name at this scope.')
param policyDefinitionName string = 'assign-kv-diag-iphub-lab-weu'

@description('Resource ID of the Log Analytics workspace that will receive Key Vault metrics.')
param logAnalytics string

@allowed(['DeployIfNotExists','Disabled'])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings name that will be created on each Key Vault.')
param profileName string = 'KeyVaultDiagnostics'

@description('Enable Key Vault audit events (AuditEvent).')
@allowed(['True', 'False'])
param auditEventEnabled string = 'True'

@description('Enable Key Vault metrics (AllMetrics).')
@allowed(['True', 'False'])
param metricsEnabled string = 'True'

resource kvDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: policyDefinitionName
  properties: {
    displayName: 'Deploy Key Vault diagnostics to Log Analytics Workspace'
    description: 'Ensures Key Vaults stream AllMetrics and AuditEvent to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/951af2fa-529b-416e-ab6e-066fd85ac459'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      diagnosticsSettingNameToUse: { value: profileName }
      logAnalytics: { value: logAnalytics }
      AuditEventEnabled: { value: auditEventEnabled }
      AllMetricsEnabled: { value: metricsEnabled }
    }
  }
}
