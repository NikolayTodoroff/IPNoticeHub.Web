/* RESOURCE MODULE: Azure Policy Initiative (Tagging)
   PROJECT: IPNoticeHub
   DATE: 2026-01-14
   
   DESCRIPTION: 
   Defines and assigns a Subscription-level Initiative that mandates four core tags:
   workload, env, owner, and region.
   
   GOVERNANCE STRATEGY: 
   Enforces metadata consistency. Without these tags, resources cannot be deployed,
   ensuring that cost-alerts (Action Groups) always have the data they need to function.
*/

targetScope = 'subscription'

// --- PARAMETERS ---

@description('Unique ID for the policy set. Matches the existing portal-generated GUID.')
param initiativeName string = '9f6793216a72425ba3080a7f'

@description('The display name and assignment name for the initiative.')
param initiativeDisplayName string = 'init-reqtags-iphub-lab-weu'

@description('Classification for the policy in the Azure Compliance dashboard.')
param category string = 'Governance'

// --- VARIABLES ---

@description('Built-in Policy Definition ID for "Require a tag on resources"')
var requireTagPolicyId = tenantResourceId('Microsoft.Authorization/policyDefinitions', '871b6d14-10aa-478d-b590-94f262ecfa99')

// --- RESOURCES ---

@description('The Policy Set (Initiative) grouping 4 instances of the Tag Requirement policy.')
resource initiative 'Microsoft.Authorization/policySetDefinitions@2021-06-01' = {
  name: initiativeName
  properties: {
    displayName: initiativeDisplayName
    policyType: 'Custom'
    description: 'Mandates 4 core tags (workload, env, owner, region) for governance.'
    metadata: {
      category: category
    }
    policyDefinitions: [
      {
        policyDefinitionReferenceId: 'req-tag-workload'
        policyDefinitionId: requireTagPolicyId
        parameters: { tagName: { value: 'workload' } }
      }
      {
        policyDefinitionReferenceId: 'req-tag-env'
        policyDefinitionId: requireTagPolicyId
        parameters: { tagName: { value: 'env' } }
      }
      {
        policyDefinitionReferenceId: 'req-tag-owner'
        policyDefinitionId: requireTagPolicyId
        parameters: { tagName: { value: 'owner' } }
      }
      {
        policyDefinitionReferenceId: 'req-tag-region'
        policyDefinitionId: requireTagPolicyId
        parameters: { tagName: { value: 'region' } }
      }
    ]
  }
}

@description('Assignment of the initiative to the Subscription scope.')
resource initiativeAssignment 'Microsoft.Authorization/policyAssignments@2022-06-01' = {
  name: 'assign-${initiativeDisplayName}'
  properties: {
    displayName: initiativeDisplayName
    policyDefinitionId: initiative.id
    enforcementMode: 'Default' // Set to 'DoNotEnforce' if you only want to audit without blocking
    nonComplianceMessages: [
      {
        message: 'Deployment blocked: Missing required tags (workload, env, owner, region). Please add tags and retry.'
      }
    ]
  }
}