/*
  Azure Policy Assignment – Storage Account SKU Governance

  Purpose:
  Enforces required tagging standards by automatically adding missing tags (workload, env, owner, region) 
  to Azure resources with predefined default values.

  Policy behavior:
  - If a resource is created/updated without one of the required tags (workload, env, owner, region), 
    the policy automatically adds it with the specified default value
  - Uses 'modify' effect to remediate resources (both new and existing via remediation tasks)
  - Excludes Azure SQL 'master' database (system resource that doesn't support tagging)

  Scope:
  Subscription (IPHub-Portfolio-Sub)
*/


targetScope = 'subscription'

@description('Location for the policy assignment managed identity.')
param location string = 'westeurope'

@description('Tag values to enforce (defaults for lab).')
param workloadTagValue string = 'ipnoticehub'
param environmentTagValue string = 'lab'
param regionTagValue string = 'weu'
param ownerTagValue string = 'nikolay'

@description('Names (override per environment if needed).')
param initiativeName string = 'init-tagging-iphub-lab-weu'
param assignmentName string = 'assign-tagging-governance'

// Built-in Role: Tag Contributor
// GUID: 4a9ae827-6dc8-4573-8ac7-8239d42aa03f
var tagContributorRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '4a9ae827-6dc8-4573-8ac7-8239d42aa03f'
)

// Exclude Azure SQL logical server system database "master" (not taggable)
var excludeSqlMasterDb = {
  not: {
    allOf: [
      { field: 'type', equals: 'Microsoft.Sql/servers/databases' }
      { field: 'name', equals: 'master' }
    ]
  }
}

// --- 1) POLICY DEFINITIONS ---
resource addWorkloadTagPolicy 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: 'pol-add-tag-workload-if-missing'
  properties: {
    displayName: 'Add required tag: workload (if missing)'
    policyType: 'Custom'
    mode: 'Indexed'
    parameters: {
      tagValue: { type: 'String' }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'tags[\'workload\']', exists: false }
          excludeSqlMasterDb
        ]
      }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [
            tagContributorRoleDefinitionId
          ]
          operations: [
            {
              operation: 'addOrReplace'
              field: 'tags[\'workload\']'
              value: '[parameters(\'tagValue\')]'
            }
          ]
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
    parameters: {
      tagValue: { type: 'String' }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'tags[\'env\']', exists: false }
          excludeSqlMasterDb
        ]
      }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [
            tagContributorRoleDefinitionId
          ]
          operations: [
            {
              operation: 'addOrReplace'
              field: 'tags[\'env\']'
              value: '[parameters(\'tagValue\')]'
            }
          ]
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
    parameters: {
      tagValue: { type: 'String' }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'tags[\'owner\']', exists: false }
          excludeSqlMasterDb
        ]
      }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [
            tagContributorRoleDefinitionId
          ]
          operations: [
            {
              operation: 'addOrReplace'
              field: 'tags[\'owner\']'
              value: '[parameters(\'tagValue\')]'
            }
          ]
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
    parameters: {
      tagValue: { type: 'String' }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'tags[\'region\']', exists: false }
          excludeSqlMasterDb
        ]
      }
      then: {
        effect: 'modify'
        details: {
          roleDefinitionIds: [
            tagContributorRoleDefinitionId
          ]
          operations: [
            {
              operation: 'addOrReplace'
              field: 'tags[\'region\']'
              value: '[parameters(\'tagValue\')]'
            }
          ]
        }
      }
    }
  }
}

// --- 2) INITIATIVE (POLICY SET) ---
resource taggingInitiative 'Microsoft.Authorization/policySetDefinitions@2021-06-01' = {
  name: initiativeName
  properties: {
    displayName: 'Initiative: Tagging Governance'
    description: 'Ensures workload, env, owner, and region tags exist (excludes Azure SQL master database).'
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

// --- 3) ASSIGNMENT ---
resource initiativeAssignment 'Microsoft.Authorization/policyAssignments@2021-06-01' = {
  name: assignmentName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
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

// --- 4) PERMISSIONS (Modify effect needs this role on the assignment identity) ---
resource tagContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(initiativeAssignment.id, tagContributorRoleDefinitionId)
  properties: {
    principalId: initiativeAssignment.identity.principalId
    roleDefinitionId: tagContributorRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}
