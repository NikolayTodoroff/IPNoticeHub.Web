/* RESOURCE MODULE: Custom Azure Policy (Location Restriction)
   PROJECT: IPNoticeHub
   DATE: 2026-01-14
   
   DESCRIPTION: 
   Creates a Custom Policy Definition and Assignment that explicitly denies 
   deployments to any region not in the approved list.
   
   SECURITY RATIONALE: 
   Hard-enforcement of regional boundaries. By including 'global', we ensure that 
   cross-region control plane resources (like Entra ID or Global Action Groups) 
   do not trigger a false denial.
*/

targetScope = 'subscription'

// --- PARAMETERS ---

@description('Policy definition GUID. Matches the existing portal-created resource for consistency.')
param policyDefinitionName string = '93c3f8b772c4bb99adde19b'

@description('The human-readable name for the assignment and portal display.')
param policyDisplayName string = 'deny-locations-iphub-lab-weu'

@description('Governance classification.')
param category string = 'Governance'

@description('List of approved regions. Global is required for non-regional resources.')
param allowedLocations array = [
  'westeurope'
  'global'
]

// --- CUSTOM POLICY DEFINITION ---

@description('The custom logic for the regional restriction.')
resource locationPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: policyDefinitionName
  properties: {
    displayName: policyDisplayName
    policyType: 'Custom'
    mode: 'All' // 'All' ensures the policy evaluates both resource groups and resources
    description: 'Enforces regional boundaries by denying any location not in the approved list.'
    metadata: {
      category: category
    }
    parameters: {
      allowedLocations: {
        type: 'Array'
        metadata: {
          description: 'The list of allowed locations for resources.'
          displayName: 'Allowed locations'
          strongType: 'location' // This provides a dropdown list in the Azure Portal UI
        }
      }
    }
    policyRule: {
      if: {
        not: {
          field: 'location'
          in: '[parameters(\'allowedLocations\')]'
        }
      }
      then: {
        effect: 'deny' // Hard enforcement: will actively block non-compliant deployments
      }
    }
  }
}

// --- POLICY ASSIGNMENT ---

@description('The actual enforcement of the custom policy at the subscription scope.')
resource locationPolicyAssignment 'Microsoft.Authorization/policyAssignments@2022-06-01' = {
  name: 'assign-${policyDisplayName}'
  properties: {
    displayName: policyDisplayName
    description: 'Restricts resource deployment to West Europe and global control-plane resources.'
    policyDefinitionId: locationPolicy.id
    enforcementMode: 'Default'
    parameters: {
      allowedLocations: {
        value: allowedLocations
      }
    }
    nonComplianceMessages: [
      {
        message: 'Deployment denied. This subscription only allows "westeurope" or "global" regions. Please correct the location and retry.'
      }
    ]
  }
}