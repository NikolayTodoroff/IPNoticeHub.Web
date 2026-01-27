/*
  Core Services Monitoring Initiative

  Purpose:
  - Creates an initiative (policy set) that ensures App Service and Key Vault have diagnostic settings and 
  auditing configured to send logs/metrics to a specified Log Analytics workspace.

  Includes:
  - Built-in: Key Vault diagnostics to Log Analytics Workspace
    /providers/Microsoft.Authorization/policyDefinitions/951af2fa-529b-416e-ab6e-066fd85ac459

  - Custom: App Service diagnostics to Log Analytics Workspace
   /providers/Microsoft.Authorization/policyDefinitions/app-service-diag-settings

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param location string
param initiativeName string
param assignmentName string

// Policy definition IDs for the included policies
var keyVaultPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/951af2fa-529b-416e-ab6e-066fd85ac459'
param appServicePolicyDefinitionId string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@description('Resource ID of the Log Analytics workspace to receive App Service and Key Vault logs/metrics.')
param logAnalytics string

@allowed(['DeployIfNotExists','Disabled'])
@description('Enable/disable the initiative.')
param effect string = 'DeployIfNotExists'

// Create the initiative (policy set)
resource coreSvsMonitoringInitiative 'Microsoft.Authorization/policySetDefinitions@2025-03-01' = {
  name: initiativeName
  properties: {
    displayName: 'Core Services Monitoring: App Service and Key Vault Log Analytics'
    description: 'Ensures App Service and Key Vault have diagnostic settings and auditing configured to send logs/metrics to the specified Log Analytics workspace.'
    policyType: 'Custom'

    parameters: {
      effect: {
        type: 'String'
        allowedValues: [ 'DeployIfNotExists','Disabled']
        defaultValue: effect
        metadata: {
          displayName: 'Effect' 
          description: 'Enable or disable policy enforcement.'
        }
      }

      logAnalytics: {
        type: 'String'
        defaultValue: logAnalytics
        metadata: {
          displayName: 'Log Analytics workspace' 
          description: 'Resource ID of the Log Analytics workspace.'
          strongType: 'omsWorkspace'
          assignPermissions: true
        }
      }

      matchWorkspace: {
        type: 'Boolean'
        defaultValue: true
        metadata: {
          displayName: 'Match Log Analytics Workspace' 
          description: 'Whether to require the workspace of the diagnostic settings matches the one deployed by this policy.'
        }
      }
    }

    policyDefinitions: [
      // Key Vault diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: keyVaultPolicyDefinitionId
        policyDefinitionReferenceId: 'key-vault-diag'
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
        }
      }

      // App Service diagnostics to Log Analytics Workspace
      {
        policyDefinitionId: appServicePolicyDefinitionId
        policyDefinitionReferenceId: 'app-service-diag'
        parameters: {
          effect: { value: '[parameters(\'effect\')]' }
          logAnalytics: {value: '[parameters(\'logAnalytics\')]'}
          matchWorkspace: {value: '[parameters(\'matchWorkspace\')]'}
        }
      }
    ]
  }
}

// Assign the initiative at subscription scope
resource coreSvsMonitoringAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'Assign Core Services Monitoring: App Service and Key Vault Log Analytics'
    description: 'Assigns core services monitoring initiative at subscription scope.'
    policyDefinitionId: coreSvsMonitoringInitiative.id
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
      logAnalytics: { value: logAnalytics }
      matchWorkspace: { value: true }
    }
  }
}

output initiativeId string = coreSvsMonitoringInitiative.id
output assignmentId string = coreSvsMonitoringAssignment.id

