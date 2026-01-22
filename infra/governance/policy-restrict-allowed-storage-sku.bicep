
/*
  Azure Policy Assignment – Storage Account SKU Governance

  Purpose:
  Enforces allowed storage account SKUs at the subscription scope to ensure
  cost control, predictable storage behavior, and consistent platform standards.

  Policy behavior:
  - Effect: Deny (built-in policy)
  - Allowed SKUs: Standard_LRS, Standard_GRS, Standard_ZRS
  - Non-compliant deployments are blocked at creation time

  Scope:
  Subscription (IPHub-Portfolio-Sub)
*/

resource storageSkuPolicyAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: 'assign-storage-allowed-skus-iphub-lab-weu'
  properties: {
    displayName: 'Limiting Allowed Storage Accounts SKUs'
    description: 'Restricts storage account deployments to approved SKUs to ensure cost-effective and predictable storage usage.\nThis policy prevents the creation of storage accounts using non-approved SKUs, helping avoid unexpected cost increases and enforcing consistent storage standards across the subscription.'
    
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/7433c107-6db4-4ad1-b57a-a76dce0154a1'

    definitionVersion: '1.*.*'

    parameters: {
      listOfAllowedSKUs: {
        value: [
          'standard_zrs'
          'standard_grs'
          'standard_lrs'
        ]
      }
    }

    enforcementMode: 'Default'
    
    nonComplianceMessages: [
  {
    message: 'This storage account uses a SKU that is not approved by subscription governance. Allowed SKUs: Standard_LRS, Standard_GRS, Standard_ZRS. Select an approved SKU or request a governance exception if required.'
  }
    ]
  }
}
