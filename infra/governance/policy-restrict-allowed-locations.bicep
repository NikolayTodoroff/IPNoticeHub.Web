/*
  Azure Policy Assignment – Restrict Allowed Locations (IPHub Lab)

  Purpose:
  Denies creation of resources outside approved deployment locations to enforce
  regional governance and reduce accidental multi-region sprawl.

  Allowed locations:
  - westeurope
  - global (required for non-regional/global resources)

  Scope:
  Subscription
*/

resource allowDeploymentLocations 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: 'assign-allowed-locations-iphub-lab-weu'
  properties: {
    displayName: 'Governance: Restrict Allowed Locations (West Europe)'
    description: 'Restricts resource deployment to West Europe for the IPHub lab subscription. Global is allowed for non-regional resources.'

    policyDefinitionId: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/Microsoft.Authorization/policyDefinitions/930c3f8b772c4bb99adde19b'

    definitionVersion: '1.*.*'

    parameters: {
      allowedLocations: {
        value: [
          'westeurope'
          'global'
        ]
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
