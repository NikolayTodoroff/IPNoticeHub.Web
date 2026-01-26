/*
  SQL Database Diagnostic Settings Policy Assignment

  Purpose:
  - Assigns a policy that ensures SQL Databases have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Unique policy assignment name at subscription scope.')
param assignmentName string = 'assign-sqldb-diag-iphub-lab-weu'

@description('Resource ID of the Log Analytics workspace that will receive SQL Database logs/metrics.')
param logAnalyticsWorkspaceResourceId string

@allowed([
  'DeployIfNotExists'
  'Disabled'
])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings name that will be created on each SQL Database.')
param diagnosticsSettingNameToUse string = 'SQLDatabaseDiagnosticsLogsToWorkspace'

@allowed(['True', 'False'])
param queryStoreRuntimeStatisticsEnabled string = 'True'

@allowed(['True', 'False'])
param queryStoreWaitStatisticsEnabled string = 'True'

@allowed(['True', 'False'])
param errorsEnabled string = 'True'

@allowed(['True', 'False'])
param databaseWaitStatisticsEnabled string = 'True'

@allowed(['True', 'False'])
param blocksEnabled string = 'True'

@allowed(['True', 'False'])
param sqlInsightsEnabled string = 'True'

@allowed(['True', 'False'])
param sqlSecurityAuditEventsEnabled string = 'True'

@allowed(['True', 'False'])
param timeoutsEnabled string = 'True'

@allowed(['True', 'False'])
param automaticTuningEnabled string = 'True'

@allowed(['True', 'False'])
param deadlocksEnabled string = 'True'

@allowed(['True', 'False'])
param basicMetricsEnabled string = 'True'

@allowed(['True', 'False'])
param instanceAndAppAdvancedMetricsEnabled string = 'True'

@allowed(['True', 'False'])
param workloadManagementMetricsEnabled string = 'True'

resource sqlDbDiagAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Deploy SQL Database diagnostic settings to Log Analytics (IPHub)'
    description: 'Ensures SQL Databases stream selected diagnostic logs and metrics to the Log Analytics workspace.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/b79fa14e-238a-4c2d-b376-442ce508fc84'
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      diagnosticsSettingNameToUse: { value: diagnosticsSettingNameToUse }
      logAnalytics: { value: logAnalyticsWorkspaceResourceId }

      QueryStoreRuntimeStatisticsEnabled: { value: queryStoreRuntimeStatisticsEnabled }
      QueryStoreWaitStatisticsEnabled: { value: queryStoreWaitStatisticsEnabled }
      ErrorsEnabled: { value: errorsEnabled }
      DatabaseWaitStatisticsEnabled: { value: databaseWaitStatisticsEnabled }
      BlocksEnabled: { value: blocksEnabled }
      SQLInsightsEnabled: { value: sqlInsightsEnabled }
      SQLSecurityAuditEventsEnabled: { value: sqlSecurityAuditEventsEnabled }
      TimeoutsEnabled: { value: timeoutsEnabled }
      AutomaticTuningEnabled: { value: automaticTuningEnabled }
      DeadlocksEnabled: { value: deadlocksEnabled }
      Basic: { value: basicMetricsEnabled }
      InstanceAndAppAdvanced: { value: instanceAndAppAdvancedMetricsEnabled }
      WorkloadManagement: { value: workloadManagementMetricsEnabled }
    }
  }
}
