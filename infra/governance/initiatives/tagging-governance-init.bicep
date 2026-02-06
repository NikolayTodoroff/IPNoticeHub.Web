/*
  Tagging Governance Initiative Assignment

  Purpose:
   - Enforces required tagging standards by automatically adding missing tags (workload, env, owner, region) 
  to Azure resources with predefined default values.
  - Excludes Azure SQL logical server system database "master" from tagging enforcement.
  - Assigns a Managed Identity to the initiative for secure operations.

  Scope:
  Subscription
*/

targetScope = 'subscription'

param initiativeName string
param assignmentName string
param location string

param workload string
param env string
param region string
param owner string

@description('Role Definition ID for Tag Contributor role')
var tagContributorRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions','4a9ae827-6dc8-4573-8ac7-8239d42aa03f')

// Excluding Azure SQL Server system database "master"
var excludeSqlMasterDb = {
  not: {
    allOf: [
      { field: 'type', equals: 'Microsoft.Sql/servers/databases' }
      { field: 'name', equals: 'master' }
    ]
  }
}

// Workload Tag Policy Definition
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
        effect: 'Modify'
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

// Env Tag Policy Definition
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
        effect: 'Modify'
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

// Owner Tag Policy Definition
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
        effect: 'Modify'
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

// Region Tag Policy Definition
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
        effect: 'Modify'
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

// Tagging Governance Initiative Definition
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

// Tagging Governance Initiative Definition Assignment
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
      workloadVal: { value: workload }
      envVal: { value: env }
      ownerVal: { value: owner }
      regionVal: { value: region }
    }
  }
}

// Role Assignment: Grant Tag Contributor role to the Initiative's Managed Identity
resource tagContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().id, initiativeAssignment.name, tagContributorRoleDefinitionId)
  scope: subscription()
  properties: {
    principalId: initiativeAssignment.identity.principalId
    roleDefinitionId: tagContributorRoleDefinitionId
    principalType: 'ServicePrincipal'
  }
}
