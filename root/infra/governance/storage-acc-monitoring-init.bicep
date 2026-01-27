/*
  Storage Account Monitoring Initiative

  Purpose:
  - Creates an initiative (policy set) that ensures Storage Accounts, Blob Services, and File Services 
  have diagnostic settings and auditing configured to send logs/metrics to a specified Log Analytics workspace.

  Includes:
  - Built-in Definition: Storage Accounts to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/59759c62-9a22-4cdf-ae64-074495983fef

  - Built-in Definition: Blob Services to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/b4fe1a3b-0715-4c6c-a5ea-ffc33cf823cb

  - Built-in Definition: File Services to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/25a70cc8-2bd4-47f1-90b6-1478e4662c96

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param location string
param initiativeName string
param assignmentName string

@description('Resource ID of the Log Analytics workspace to receive Storage Account logs/metrics.')
param logAnalytics string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed([
  'DeployIfNotExists'
  'AuditIfNotExists'
  'Disabled'
])
@description('Enable/disable the initiative.')
param effect string = 'DeployIfNotExists'

@description('Whether to enable metrics streaming to Log Analytics for Storage Accounts, Blob Services, and File Services.')
param metricsEnabled bool = true

@description('Whether to enable logs streaming to Log Analytics for Storage Accounts, Blob Services, and File Services.')
param logsEnabled bool = true

@description('Diagnostic settings profile name that will be created on each Storage Account.')
param storageProfileName string = 'storageAccountsDiagnosticsLogsToWorkspace'

@description('Diagnostic settings profile name that will be created on each Blob Service.')
param blobProfileName string = 'blobServicesDiagnosticsLogsToWorkspace'

@description('Diagnostic settings profile name that will be created on each File Service.')
param fileProfileName string = 'fileServicesDiagnosticsLogsToWorkspace'

// Policy definition IDs for the included policies
var storageAccPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/59759c62-9a22-4cdf-ae64-074495983fef'
var blobSvsPolicyDefinitionId    = '/providers/Microsoft.Authorization/policyDefinitions/b4fe1a3b-0715-4c6c-a5ea-ffc33cf823cb'
var fileSvsPolicyDefinitionId    = '/providers/Microsoft.Authorization/policyDefinitions/25a70cc8-2bd4-47f1-90b6-1478e4662c96'

resource storAccMonitoringInitiative 'Microsoft.Authorization/policySetDefinitions@2025-03-01' = {
  name: initiativeName
  properties: {
    displayName: 'Storage Monitoring: Storage, Blob and File Services to Log Analytics Workspace'
    description: 'Ensures Storage Accounts, Blob Services, and File Services stream diagnostics to Log Analytics.'
    policyType: 'Custom'

    parameters: {
      effect: {
        type: 'String'
        allowedValues: [ 'DeployIfNotExists','AuditIfNotExists','Disabled']
        defaultValue: 'DeployIfNotExists'
        metadata: {displayName: 'Effect', description: 'Enable or disable policy enforcement.'}
      }

      logAnalytics: {
        type: 'String'
        metadata: {displayName: 'Log Analytics workspace', description: 'Resource ID of the Log Analytics workspace.'}
      }

      storageProfileName: { 
        type: 'String'
        defaultValue: 'storageAccountsDiagnosticsLogsToWorkspace' 
        metadata: {displayName: 'Storage Account diagnostics profile name', description: 'Name of the diagnostic settings profile for Storage Accounts.'}
      }

      blobProfileName: { 
        type: 'String'
        defaultValue: 'blobServicesDiagnosticsLogsToWorkspace' 
        metadata: {displayName: 'Blob Service diagnostics profile name', description: 'Name of the diagnostic settings profile for Blob Services.'}
      }

      fileProfileName: { 
        type: 'String'
        defaultValue: 'fileServicesDiagnosticsLogsToWorkspace' 
        metadata: {displayName: 'File Service diagnostics profile name', description: 'Name of the diagnostic settings profile for File Services.'}
      }
      metricsEnabled: { 
        type: 'Boolean'
        defaultValue: true 
        metadata: {displayName: 'Enable metrics', description: 'Enable or disable metrics collection for Storage Accounts.'}
      }

      logsEnabled: { 
        type: 'Boolean'
        defaultValue: true 
        metadata: {displayName: 'Enable logs', description: 'Enable or disable logs collection for Storage Accounts.'}
      }
    }

    policyDefinitions: [
      // Storage Account diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: storageAccPolicyDefinitionId
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          profileName: { value: '[parameters(\'storageProfileName\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
          metricsEnabled: { value: '[parameters(\'metricsEnabled\')]' }
        }
      }

      // Blob Service diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: blobSvsPolicyDefinitionId
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          profileName: { value: '[parameters(\'blobProfileName\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
          metricsEnabled: { value: '[parameters(\'metricsEnabled\')]' }
          logsEnabled: { value: '[parameters(\'logsEnabled\')]' }
        }
      }

      // File Service diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: fileSvsPolicyDefinitionId
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          profileName: { value: '[parameters(\'fileProfileName\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
          metricsEnabled: { value: '[parameters(\'metricsEnabled\')]' }
          logsEnabled: { value: '[parameters(\'logsEnabled\')]' }
        }
      }
    ]
  }
}

// Assign the initiative at subscription scope
resource storAccMonitoringAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }
  properties: {
    displayName: 'Storage Monitoring Assignment (Storage Account/Blob Service/File Service)'
    description: 'Assigns the Storage Monitoring initiative to ensure Storage Accounts, Blob Services, and File Services stream diagnostics to Log Analytics.'
    policyDefinitionId: storAccMonitoringInitiative.id
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      logAnalytics: { value: logAnalytics }
      storageProfileName: { value: storageProfileName }
      blobProfileName: { value: blobProfileName }
      fileProfileName: { value: fileProfileName }
      metricsEnabled: { value: metricsEnabled }
      logsEnabled: { value: logsEnabled }
    }
  }
}

