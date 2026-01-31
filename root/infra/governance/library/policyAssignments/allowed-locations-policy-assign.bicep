/*
  Allowed Locations Policy Assignment

  Purpose:
   - Restricts resource deployment to approved locations to enforce regional governance and reduce accidental multi-region sprawl.
   - Allowed locations westeurope and global
   - Uses Indexed mode to avoid sub-resources without locations.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param assignmentName string

@description('The list of allowed locations for resource deployment.')
param allowedLocations array = ['westeurope','global']

// Allowed Locations Policy Definition
resource locationPolicyDefinition 'Microsoft.Authorization/policyDefinitions@2025-03-01' = {
  name: 'restrict-locations-indexed'
  properties: {
    displayName: 'Governance: Restrict Allowed Locations (Indexed)'
    description: 'Restricts deployments to approved locations for regional resources. Uses Indexed mode to avoid sub-resources without locations.'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: {
      allowedLocations: {
        type: 'Array'
        metadata: {
          displayName: 'Allowed Locations'
          description: 'The list of allowed locations for resources.'
        }
      }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'location', exists: 'true' }
          { field: 'location', notIn: '[parameters(\'allowedLocations\')]' }
        ]
      }
      then: {
        effect: 'Deny'
      }
    }
  }
}

// Allowed Locations Policy Definition Assignment
resource allowDeploymentLocations 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Governance: Restrict Allowed Locations (West Europe)'
    description: 'Restricts resource deployment to West Europe and Global using Indexed mode.'
    policyDefinitionId: locationPolicyDefinition.id
    parameters: {
      allowedLocations: {
        value: allowedLocations
      }
    }
    enforcementMode: 'Default'
    nonComplianceMessages: [
      {
        message: 'Deployment denied. This subscription allows resource deployment only in West Europe (westeurope) and global. Choose an approved location or request a governance exception.'
      }
    ]
  }
}
