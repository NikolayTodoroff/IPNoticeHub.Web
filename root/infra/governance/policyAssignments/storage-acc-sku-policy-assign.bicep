/*
  Storage Account SKU Governance Policy Assignment

  Purpose:
  - Enforces allowed storage account SKUs at the subscription scope to ensure
  cost control, predictable storage behavior, and consistent platform standards.
  - Effect: Deny
  - Allowed SKUs: Standard_LRS, Standard_GRS, Standard_ZRS

  Scope:
  - Subscription (IPHub-Portfolio-Sub)
*/

targetScope = 'subscription'

param assignmentName string
param allowedSkus array = [
  'standard_lrs'
  'standard_grs'
  'standard_zrs'
]

resource storageSkuPolicyAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'Storage Account SKU Governance'
    description: 'Restricts storage account deployments to approved SKUs to ensure cost-effective and predictable storage usage.\nThis policy prevents the creation of storage accounts using non-approved SKUs, helping avoid unexpected cost increases and enforcing consistent storage standards across the subscription.'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/7433c107-6db4-4ad1-b57a-a76dce0154a1'

    parameters: {
      listOfAllowedSKUs: {
        value: allowedSkus
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
