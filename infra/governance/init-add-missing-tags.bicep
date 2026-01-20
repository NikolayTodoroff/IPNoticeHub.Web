/*
  Azure Policy Deployment: Tagging Governance
  Purpose: Automatically adds 'workload', 'env', 'owner', and 'region' tags if missing.
  Author: Nikolay
  Scope: Subscription
*/

targetScope = 'subscription'

@description('Location for the policy assignments managed identities.')
param location string = 'westeurope'

@description('Tag values from your requirements.')
param workloadTagValue string = 'ipnoticehub'
param environmentTagValue string = 'lab'
param regionTagValue string = 'weu'
param ownerTagValue string = 'nikolay'

// Role ID for 'Tag Contributor' - allows the policy to add tags to resources
var tagContributorRole = '/providers/Microsoft.Authorization/roleDefinitions/4a9ae827-6dc8-4573-8ac7-8239d42aa03f'

// --- 1. POLICY DEFINITIONS ---

resource addWorkloadTagPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: 'pol-add-tag-workload-if-missing'
  properties: {
    displayName: 'Add required tag: workload (if missing)'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: { tagValue: { type: 'String' } }
    policyRule: {
      if: { field: 'tags[\'workload\']', exists: false }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [ tagContributorRole ]
          operations: [ { operation: 'add', field: 'tags[\'workload\']', value: '[parameters(\'tagValue\')]' } ]
        }
      }
    }
  }
}

resource addEnvTagPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: 'pol-add-tag-env-if-missing'
  properties: {
    displayName: 'Add required tag: env (if missing)'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: { tagValue: { type: 'String' } }
    policyRule: {
      if: { field: 'tags[\'env\']', exists: false }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [ tagContributorRole ]
          operations: [ { operation: 'add', field: 'tags[\'env\']', value: '[parameters(\'tagValue\')]' } ]
        }
      }
    }
  }
}

resource addOwnerTagPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: 'pol-add-tag-owner-if-missing'
  properties: {
    displayName: 'Add required tag: owner (if missing)'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: { tagValue: { type: 'String' } }
    policyRule: {
      if: { field: 'tags[\'owner\']', exists: false }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [ tagContributorRole ]
          operations: [ { operation: 'add', field: 'tags[\'owner\']', value: '[parameters(\'tagValue\')]' } ]
        }
      }
    }
  }
}

resource addRegionTagPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: 'pol-add-tag-region-if-missing'
  properties: {
    displayName: 'Add required tag: region (if missing)'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: { tagValue: { type: 'String' } }
    policyRule: {
      if: { field: 'tags[\'region\']', exists: false }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [ tagContributorRole ]
          operations: [ { operation: 'add', field: 'tags[\'region\']', value: '[parameters(\'tagValue\')]' } ]
        }
      }
    }
  }
}

// --- 2. INITIATIVE ---

resource taggingInitiative 'Microsoft.Authorization/policySetDefinitions@2021-06-01' = {
  name: 'init-tagging-iphub-lab-weu'
  properties: {
    displayName: 'Initiative: Tagging Governance'
    description: 'Enforces workload, env, owner, and region tags.'
    policyType: 'Custom'
    metadata: { category: 'Tags' }
    parameters: {
      workloadVal: { type: 'String' }
      envVal: { type: 'String' }
      ownerVal: { type: 'String' }
      regionVal: { type: 'String' }
    }
    policyDefinitions: [
      {
        policyDefinitionId: addWorkloadTagPolicy.id
        parameters: { tagValue: { value: '[parameters(\'workloadVal\')]' } }
      }
      {
        policyDefinitionId: addEnvTagPolicy.id
        parameters: { tagValue: { value: '[parameters(\'envVal\')]' } }
      }
      {
        policyDefinitionId: addOwnerTagPolicy.id
        parameters: { tagValue: { value: '[parameters(\'ownerVal\')]' } }
      }
      {
        policyDefinitionId: addRegionTagPolicy.id
        parameters: { tagValue: { value: '[parameters(\'regionVal\')]' } }
      }
    ]
  }
}

// --- 3. ASSIGNMENT ---

resource initiativeAssignment 'Microsoft.Authorization/policyAssignments@2021-06-01' = {
  name: 'assign-tagging-governance'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    displayName: 'Assignment: Tagging Governance Initiative'
    policyDefinitionId: taggingInitiative.id
    parameters: {
      workloadVal: { value: workloadTagValue }
      envVal: { value: environmentTagValue }
      ownerVal: { value: ownerTagValue }
      regionVal: { value: regionTagValue }
    }
  }
}

// --- 4. PERMISSIONS (Crucial for Modify Effect) ---

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(initiativeAssignment.id, tagContributorRole)
  properties: {
    principalId: initiativeAssignment.identity.principalId
    roleDefinitionId: tagContributorRole
    principalType: 'ServicePrincipal'
  }
}
