/*
  SQL Monitoring Initiative

  Purpose:
  - Creates an initiative (policy set) that ensures SQL Servers and SQL Databases have diagnostic settings and 
  auditing configured to send logs/metrics to a specified Log Analytics workspace.

  Includes:
  - Built-in: SQL Database diagnostics to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/b79fa14e-238a-4c2d-b376-442ce508fc84

  - Built-in: SQL Server auditing + SQLSecurityAuditEvents to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/7ea8a143-05e3-4553-abfe-f56bef8b0b70

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param location string
param initiativeName string
param assignmentName string

@description('Resource ID of the Log Analytics workspace to receive SQL logs/metrics.')
param logAnalytics string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed(['DeployIfNotExists','Disabled'])
@description('Enable/disable the initiative.')
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings name to use for SQL Databases.')
param sqlDbDiagnosticsSettingNameToUse string = 'SQLDatabaseDiagnosticsLogsToWorkspace'

// Policy definition IDs for the included policies
var sqlDbPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/b79fa14e-238a-4c2d-b376-442ce508fc84'
var sqlServerPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/7ea8a143-05e3-4553-abfe-f56bef8b0b70'

// Create the initiative (policy set)
resource sqlMonitoringInitiative 'Microsoft.Authorization/policySetDefinitions@2025-03-01' = {
  name: initiativeName
  properties: {
    displayName: 'SQL Monitoring: SQL Servers and SQL Databases Log Analytics'
    description: 'Ensures SQL Servers (auditing) and SQL Databases (diagnostics) stream to the specified Log Analytics workspace.'
    policyType: 'Custom'

    parameters: {
      effect: {
        type: 'String'
        allowedValues: [ 'DeployIfNotExists','Disabled']
        defaultValue: 'DeployIfNotExists'
        metadata: {displayName: 'Effect', description: 'Enable or disable policy enforcement.'}
      }

      logAnalytics: {
        type: 'String'
        metadata: {displayName: 'Log Analytics workspace', description: 'Resource ID of the Log Analytics workspace.'}
      }

      sqlDbDiagnosticsSettingNameToUse: {
        type: 'String'
        defaultValue: 'SQLDatabaseDiagnosticsLogsToWorkspace'
        metadata: {displayName: 'SQL DB diagnostic setting name', description: 'Name of the diagnostic settings created on each SQL Database.'}
      }
    }

    policyDefinitions: [
      // SQL Databases diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: sqlDbPolicyDefinitionId
        policyDefinitionReferenceId: 'sql-db-diag'
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
          diagnosticsSettingNameToUse: { value: '[parameters(\'sqlDbDiagnosticsSettingNameToUse\')]' }
        }
      }

      // SQL Servers auditing to Log Analytics Workspace
      {
        policyDefinitionId: sqlServerPolicyDefinitionId
        policyDefinitionReferenceId: 'sql-server-audit'
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
        }
      }
    ]
  }
}

// Assign the initiative at subscription scope
resource sqlMonitoringAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'Assign SQL Monitoring: SQL Servers and SQL Databases Log Analytics'
    description: 'Assigns SQL monitoring initiative at subscription scope.'
    policyDefinitionId: sqlMonitoringInitiative.id
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      logAnalytics: { value: logAnalytics }
      sqlDbDiagnosticsSettingNameToUse: { value: sqlDbDiagnosticsSettingNameToUse }
    }
  }
}

output initiativeId string = sqlMonitoringInitiative.id
output assignmentId string = sqlMonitoringAssignment.id
