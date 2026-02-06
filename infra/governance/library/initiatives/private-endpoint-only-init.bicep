/*
  SQL Server,Key Vault, and Storage Account Public Access Disable Initiative Assignment

  Purpose:
  - Creates an initiative assignment that ensures SQL Servers, Key Vaults, and Storage Accounts have public access disabled.
  
  Includes:
  - Built-in: SQL Servers should disallow public network access
    /providers/Microsoft.Authorization/policyDefinitions/28b0b1e5-17ba-4963-a7a4-5a1ab4400a0b

  - Built-in: Key Vaults should disallow public network access
    /providers/Microsoft.Authorization/policyDefinitions/405c5871-3e91-4644-8a63-58e19d68ff5b

  - Built-in: Storage Accounts should disallow public network access
    /providers/Microsoft.Authorization/policyDefinitions/a06d0189-92e8-4dba-b0c4-08d7669fce7d

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param location string
param initiativeName string
param assignmentName string

@description('Resource ID of the User Assigned Managed Identity used for Azure Policy remediation.')
param policyRemediationUamiResourceId string

@allowed(['Modify','Disabled'])
@description('Enable/disable the policy assignment.')
param effectModify string = 'Modify'

@allowed(['Audit','Deny','Disabled'])
@description('Enable/disable the policy assignment.')
param effectAudit string = 'Audit'

// Policy definition IDs for the included policies
var sqlServerPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/28b0b1e5-17ba-4963-a7a4-5a1ab4400a0b'
var keyVaultPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/405c5871-3e91-4644-8a63-58e19d68ff5b'
var storageAccPolicyDefinitionId = '/providers/Microsoft.Authorization/policyDefinitions/a06d0189-92e8-4dba-b0c4-08d7669fce7d'

// Public Access Disabled Initiative Definition
resource privateEndpointOnlyInitiative 'Microsoft.Authorization/policySetDefinitions@2025-03-01' = {
  name: initiativeName
  properties: {
    displayName: 'SQL Server, Key Vault, and Storage Account Private Endpoint Access Only'
    description: 'Ensures SQL Servers, Key Vaults, and Storage Accounts have private endpoint access only.'
    policyType: 'Custom'

    parameters: {
      effectModify: {
        type: 'String'
        allowedValues: [ 'Modify','Disabled']
        defaultValue: 'Modify'
        metadata: {
          displayName: 'Modify Effect'
          description: 'Enable or disable policy enforcement.'}
      }

      effectAudit: {
        type: 'String'
        allowedValues: [ 'Audit','Deny','Disabled']
        defaultValue: 'Audit'
        metadata: {
          displayName: 'Audit Effect'
          description: 'Enable or disable policy auditing.'}
      }
    }

    policyDefinitions: [
      // SQL Server Private Endpoint Access Only
      {
        policyDefinitionId: sqlServerPolicyDefinitionId
        policyDefinitionReferenceId: 'sql-server-private-endpoint-only'
        parameters: {
          effect: { value: '[parameters(\'effectModify\')]' }
        }
      }

      // Storage Account Private Endpoint Access Only
      {
        policyDefinitionId: storageAccPolicyDefinitionId
        policyDefinitionReferenceId: 'storage-account-private-endpoint-only'
        parameters: {
          effect: { value: '[parameters(\'effectModify\')]' }
        }
      }

      // Key Vault Private Endpoint Access Only
      {
        policyDefinitionId: keyVaultPolicyDefinitionId
        policyDefinitionReferenceId: 'key-vault-private-endpoint-only'
        parameters: {
          effect: { value: '[parameters(\'effectAudit\')]' }
        }
      }
    ]
  }
}

// SQL Server, Key Vault, and Storage Account Private Endpoint Access Only Initiative Assignment
resource privateEndpointOnlyAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${policyRemediationUamiResourceId}': {}
    }
  }

  properties: {
    displayName: 'Private Endpoint Access Only Initiative'
    description: 'Assigns SQL Server, Key Vault, and Storage Account Private Endpoint Access Only initiative at subscription scope.'
    policyDefinitionId: privateEndpointOnlyInitiative.id
    enforcementMode: 'Default'
    parameters: {
      effectModify: { value: effectModify }
      effectAudit: { value: effectAudit }
    }
  }
}
