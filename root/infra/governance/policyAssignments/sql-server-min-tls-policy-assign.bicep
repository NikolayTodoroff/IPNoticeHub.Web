/*
  SQL Server Minimum TLS Version Policy Assignment

  Purpose:
  - Enforces a minimum TLS version('1.0','1.1','1.2') for Azure SQL logical servers.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param assignmentName string

@description('The name of the policy definition.')
param policyName string = 'pol-sql-min-tls'

@allowed(['Deny','Audit','Disabled'])
param effect string = 'Audit'

@allowed(['1.0','1.1','1.2'])
@description('The minimum TLS version to enforce.')
param minTlsVersion string = '1.2'

// SQL Server Minimum TLS Policy Definition
resource sqlMinTlsDef 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: policyName
  properties: {
    displayName: 'SQL: enforce minimum TLS version'
    description: 'Denies/Audits SQL servers that do not enforce the required minimum TLS version.'
    policyType: 'Custom'
    mode: 'Indexed'
    metadata: {
      category: 'Security'
    }
    parameters: {
      effect: {
        type: 'String'
        allowedValues: ['Deny','Audit','Disabled']    
        defaultValue: effect
      }
      minTlsVersion: {
        type: 'String'
        allowedValues: ['1.0','1.1','1.2']
        defaultValue: minTlsVersion
      }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'type', equals: 'Microsoft.Sql/servers' }
          { field: 'Microsoft.Sql/servers/minimalTlsVersion', notEquals: '[parameters(\'minTlsVersion\')]' }
        ]
      }
      then: {
        effect: '[parameters(\'effect\')]'
      }
    }
  }
}

// SQL Server Minimum TLS Policy Assignment
resource sqlMinTlsAssign 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'SQL: minimum TLS (subscription)'
    description: 'Enforces minimum TLS version on SQL servers.'
    policyDefinitionId: sqlMinTlsDef.id
    parameters: {
      effect: { value: effect }
      minTlsVersion: { value: minTlsVersion }
    }
    nonComplianceMessages: [
      { 
        message: 'SQL servers must enforce minimum TLS version 1.2.' 
      }
    ]
  }
}
