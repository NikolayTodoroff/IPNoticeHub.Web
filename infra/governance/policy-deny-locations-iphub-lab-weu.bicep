targetScope = 'subscription'

/*
  Mirrors the existing custom policy definition created in portal:
  - ID: /subscriptions/.../policyDefinitions/93c3f8b772c4bb99adde19b
  - DisplayName: deny-locations-iphub-lab-weu
  - Effect: audit (as shown in your definition)
*/

@description('Policy definition name. Keeping the Azure-generated GUID keeps it identical to the portal-created resource.')
param policyDefinitionName string = '93c3f8b772c4bb99adde19b'

@description('Policy assignment name at subscription scope.')
param policyAssignmentName string = 'deny-locations-iphub-lab-weu'

@description('Policy display name shown in Azure Portal.')
param policyDisplayName string = 'deny-locations-iphub-lab-weu'

@description('Policy description shown in Azure Portal.')
param policyDescription string = 'Denies deployment of resources outside West Europe for IPHub lab subscription.'

@description('Policy category metadata.')
param category string = 'Governance'

@description('Allowed locations for the assignment. Include global for global resources like Action Groups.')
param allowedLocations array = [
  'westeurope'
  'global'
]

resource locationPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: policyDefinitionName
  properties: {
    displayName: policyDisplayName
    policyType: 'Custom'
    mode: 'All'
    description: policyDescription
    metadata: {
      category: category
    }
    version: '1.0.0'
    parameters: {
      allowedLocations: {
        type: 'Array'
        metadata: {
          description: 'The list of allowed locations for resources.'
          displayName: 'Allowed locations'
          strongType: 'location'
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
        // Mirror current definition effect (Audit). Change to 'Deny' later if desired.
        effect: 'deny'
      }
    }
  }
}

resource locationPolicyAssignment 'Microsoft.Authorization/policyAssignments@2022-06-01' = {
  name: policyAssignmentName
  properties: {
    displayName: policyDisplayName
    description: 'Restricts resource deployment to West Europe (and global control-plane resources) for the IPHub lab subscription.'
    policyDefinitionId: locationPolicy.id
    enforcementMode: 'Default'
    parameters: {
      allowedLocations: {
        value: allowedLocations
      }
    }
    nonComplianceMessages: [
      {
        message: 'Deployment denied. Invalid location. This subscription allows resource deployment only in West Europe (westeurope) and global. Select a supported region and retry the deployment.'
      }
    ]
  }
}
