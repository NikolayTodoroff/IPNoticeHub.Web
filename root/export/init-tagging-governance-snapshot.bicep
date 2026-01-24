
@description('Generated from /subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/microsoft.authorization/policyassignments/assign-tagging-governance')
resource assigntagginggovernance 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    displayName: 'Tagging Governance Initiative'
    policyDefinitionId: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/Microsoft.Authorization/policySetDefinitions/init-tagging-iphub-lab-weu'
    definitionVersion: '1.*.*'
    parameters: {
      workloadVal: {
        value: 'ipnoticehub'
      }
      envVal: {
        value: 'lab'
      }
      ownerVal: {
        value: 'nikolay'
      }
      regionVal: {
        value: 'weu'
      }
    }
    metadata: {
      createdBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      createdOn: '2026-01-20T16:28:26.6317928Z'
      updatedBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      updatedOn: '2026-01-21T23:05:00.6790787Z'
      parameterScopes: {}
    }
    enforcementMode: 'Default'
    nonComplianceMessages: []
    resourceSelectors: []
    overrides: []
  }
  name: 'assign-tagging-governance'
  location: 'westeurope'
}
