/*
  Azure Policy Assignment – Restrict Allowed Locations (IPHub Lab)

  Purpose:
   - Restricts resource deployment to approved locations to enforce
     regional governance and reduce accidental multi-region sprawl.
   - Effect: Deny
   - Allowed locations westeurope and global
   - Uses Indexed mode to avoid sub-resources without locations.

  Scope:
  Subscription
*/

targetScope = 'subscription'

param assignmentName string
param allowedLocations array = [
  'westeurope'
  'global'
]

// Policy Definition (Indexed)
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

// Policy Assignment
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
