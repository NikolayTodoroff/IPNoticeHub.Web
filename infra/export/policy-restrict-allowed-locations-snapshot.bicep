
@description('Generated from /subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/microsoft.authorization/policyassignments/f13aecbdbf604968958065bf')
resource faecbdbfbf 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  properties: {
    displayName: 'Governance: Restrict Allowed Locations (West Europe)'
    policyDefinitionId: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/Microsoft.Authorization/policyDefinitions/930c3f8b772c4bb99adde19b'
    definitionVersion: '1.*.*'
    notScopes: []
    parameters: {
      allowedLocations: {
        value: [
          'westeurope'
          'global'
        ]
      }
    }
    description: 'Restricts resource deployment to West Europe for the IPHub lab subscription.\nThis policy enforces geographic governance by denying creation of resources outside the approved region (westeurope).'
    metadata: {
      parameterScopes: {}
      createdBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      createdOn: '2026-01-12T17:46:26.4036615Z'
      updatedBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      updatedOn: '2026-01-20T17:26:20.7470122Z'
    }
    enforcementMode: 'Default'
    nonComplianceMessages: [
      {
        message: 'Deployment denied. Invalid location.\nThis subscription allows resource deployment only in West Europe (westeurope).\nSelect a supported region and retry the deployment.'
      }
    ]
    resourceSelectors: []
    overrides: []
  }
  name: 'f13aecbdbf604968958065bf'
}
