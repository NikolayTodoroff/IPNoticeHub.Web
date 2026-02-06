
@description('Generated from /subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/Microsoft.Authorization/policyAssignments/5d7284690fb04b6994b369e2')
resource dfbbbe 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  properties: {
    displayName: 'Limiting Allowed Storage Accounts SKUs'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/7433c107-6db4-4ad1-b57a-a76dce0154a1'
    definitionVersion: '1.*.*'
    notScopes: []
    parameters: {
      listOfAllowedSKUs: {
        value: [
          'standard_zrs'
          'standard_grs'
          'standard_lrs'
        ]
      }
    }
    description: 'Restricts storage account deployments to approved SKUs to ensure cost-effective and predictable storage usage.\nThis policy prevents the creation of storage accounts using non-approved SKUs, helping avoid unexpected cost increases and enforcing consistent storage standards across the subscription.'
    metadata: {
      parameterScopes: {
        listOfAllowedSKUs: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07'
      }
      createdBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      createdOn: '2026-01-21T23:00:06.488746Z'
      updatedBy: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      updatedOn: '2026-01-22T08:02:01.64601Z'
    }
    enforcementMode: 'Default'
    nonComplianceMessages: [
      {
        message: 'This storage account uses a SKU that is not approved by subscription governance. Allowed SKUs: Standard_LRS, Standard_GRS, Standard_ZRS. Select an approved SKU or request a governance exception if required.'
      }
    ]
    resourceSelectors: []
    overrides: []
  }
  name: '5d7284690fb04b6994b369e2'
}
