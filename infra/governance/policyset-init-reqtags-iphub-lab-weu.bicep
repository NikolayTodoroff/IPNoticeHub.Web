targetScope = 'subscription'

/*
  Mirrors the existing initiative:
  - ID: /subscriptions/.../policySetDefinitions/9f6793216a72425ba3080a7f
  - DisplayName: init-reqtags-iphub-lab-weu
  - 4x built-in policy "Require a tag on resources" (871b6d14-10aa-478d-b590-94f262ecfa99)
*/

@description('Subscription-scoped initiative (policy set) definition name. Keeping the Azure-generated GUID keeps it identical to the portal-created resource.')
param initiativeName string = '9f6793216a72425ba3080a7f'

@description('Initiative assignment name at subscription scope.')
param initiativeAssignmentName string = 'init-reqtags-iphub-lab-weu'

@description('Initiative display name shown in Azure Portal.')
param initiativeDisplayName string = 'init-reqtags-iphub-lab-weu'

@description('Initiative description shown in Azure Portal.')
param initiativeDescription string = 'Initiative: Enforce required resource tags (workload, env, owner, region) for IPHub in lab (West Europe).'

@description('Initiative category metadata.')
param category string = 'Governance'

// Built-in policy definition used by the initiative JSON (same ID repeated 4 times)
var requireTagOnResourcesPolicyId = '/providers/Microsoft.Authorization/policyDefinitions/871b6d14-10aa-478d-b590-94f262ecfa99'

// Optional group
var groupName = 'tagging-core'
var groupDisplayName = 'Core tagging requirements'

resource initiative 'Microsoft.Authorization/policySetDefinitions@2021-06-01' = {
  name: initiativeName
  properties: {
    displayName: initiativeDisplayName
    policyType: 'Custom'
    description: initiativeDescription
    metadata: {
      category: category
    }
    version: '1.0.0'
    parameters: {}

    policyDefinitionGroups: [
      {
        name: groupName
        category: category
        displayName: groupDisplayName
      }
    ]

    policyDefinitions: [
      {
        policyDefinitionReferenceId: 'Require a tag on resources_1'
        policyDefinitionId: requireTagOnResourcesPolicyId
        parameters: {
          tagName: { value: 'workload' }
        }
        groupNames: []
      }
      {
        policyDefinitionReferenceId: 'Require a tag on resources_2'
        policyDefinitionId: requireTagOnResourcesPolicyId
        parameters: {
          tagName: { value: 'env' }
        }
        groupNames: []
      }
      {
        policyDefinitionReferenceId: 'Require a tag on resources_3'
        policyDefinitionId: requireTagOnResourcesPolicyId
        parameters: {
          tagName: { value: 'owner' }
        }
        groupNames: []
      }
      {
        policyDefinitionReferenceId: 'Require a tag on resources_4'
        policyDefinitionId: requireTagOnResourcesPolicyId
        parameters: {
          tagName: { value: 'region' }
        }
        groupNames: []
      }
    ]
  }
}

resource initiativeAssignment 'Microsoft.Authorization/policyAssignments@2022-06-01' = {
  name: initiativeAssignmentName
  properties: {
    displayName: initiativeDisplayName
    description: 'Enforces required resource tags (workload, env, owner, region) for IPHub lab resources in West Europe.'
    policyDefinitionId: initiative.id
    enforcementMode: 'Default'
    nonComplianceMessages: [
      {
        message: 'Policy violation: required resource tags missing. This subscription enforces mandatory tags (workload, env, owner, region) for governance and cost tracking. Add the missing tag(s) and retry the deployment.'
      }
    ]
  }
}
