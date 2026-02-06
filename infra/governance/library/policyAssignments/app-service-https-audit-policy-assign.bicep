/*
  Azure Policy Assignment — App Service HTTPS-only

  Purpose:
  - Enforces/Audits HTTPS-only for Azure App Service apps.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param assignmentName string
param policyName string = 'pol-appservice-https-only'

@allowed(['Deny','Audit','Disabled'])
param effect string = 'Audit'

resource appServiceHttpsOnlyDef 'Microsoft.Authorization/policyDefinitions@2021-06-01' = {
  name: policyName
  properties: {
    displayName: 'App Service: enforce HTTPS-only'
    description: 'Enforces/Audits App Service apps that do not enforce HTTPS-only.'
    policyType: 'Custom'
    mode: 'Indexed'
    metadata: {
      category: 'Security'
    }
    parameters: {
      effect: {
        type: 'String'
        allowedValues: ['Deny','Audit','Disabled']
        defaultValue: effect
      }
    }
    policyRule: {
      if: {
        allOf: [
          { field: 'type', equals: 'Microsoft.Web/sites' }
          { field: 'Microsoft.Web/sites/httpsOnly', notEquals: true }
        ]
      }
      then: {
        effect: '[parameters(\'effect\')]'
      }
    }
  }
}

resource appServiceHttpsOnlyAssign 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service: HTTPS-only enforcement'
    description: 'Enforces/Audits HTTPS-only on App Service apps.'
    policyDefinitionId: appServiceHttpsOnlyDef.id
    parameters: {
      effect: { value: effect }
    }
    nonComplianceMessages: [
      {
        message: 'App Service apps must enforce HTTPS-only.'
      }
    ]
  }
}
