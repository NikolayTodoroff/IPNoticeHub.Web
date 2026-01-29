/*
  App Service Local Authentication Methods Disabled Initiative

  Purpose:
  - Creates an initiative to ensure that app service have local authentication methods 
  for both FTP and SCM disabled for security.

  Includes:
  - Built-in: App Service FTP Authentication Disabled Audit Policy
    /providers/Microsoft.Authorization/policyDefinitions/871b205b-57cf-4e1e-a234-492616998bf7

 - Built-in: App Service SCM Authentication Disabled Audit Policy
    /providers/Microsoft.Authorization/policyDefinitions/aede300b-d67f-480a-ae26-4b3dfb1a1fdc

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param location string
param initiativeName string
param assignmentName string

@description('Resource ID of the App Service FTP Authentication Disabled Audit Policy.')
param ftpAuthPolicyDefinitionId string ='/providers/Microsoft.Authorization/policyDefinitions/871b205b-57cf-4e1e-a234-492616998bf7'

@description('Resource ID of the App Service SCM Authentication Disabled Audit Policy.')
param scmAuthPolicyDefinitionId string ='/providers/Microsoft.Authorization/policyDefinitions/aede300b-d67f-480a-ae26-4b3dfb1a1fdc'

@allowed(['AuditIfNotExists','Disabled'])
param effect string = 'AuditIfNotExists'

// Create the initiative (policy set)
resource appServiceAuthInit 'Microsoft.Authorization/policySetDefinitions@2025-03-01' = {
  name: initiativeName
  properties: {
    displayName: 'App Service Local Authentication Methods Disabled Initiative'
    description: 'Ensures App Service have local authentication methods for both FTP and SCM disabled for security.'
    policyType: 'Custom'

    parameters: {
      effect: {
        type: 'String'
        allowedValues: [ 'AuditIfNotExists','Disabled']
        defaultValue: effect
        metadata: {
          displayName: 'Effect' 
          description: 'Enable or disable policy enforcement.'
        }
      }
    }

    policyDefinitions: [
      {
        policyDefinitionId: ftpAuthPolicyDefinitionId
        policyDefinitionReferenceId: 'app-service-ftp-auth'
        parameters: {
          effect: {
            value: '[parameters(\'effect\')]'
          }
        }
      }
      {
        policyDefinitionId: scmAuthPolicyDefinitionId
        policyDefinitionReferenceId: 'app-service-scm-auth'
        parameters: {
          effect: {
            value: '[parameters(\'effect\')]'
          }
        }
      }
    ]
  }
}

// Assign the initiative at subscription scope
resource appServiceAuthInitAssignment 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  location: location

  properties: {
    displayName: 'Assign App Service Local Authentication Methods Disabled Audit Initiative'
    description: 'Assigns app service local authentication methods disabled audit initiative at subscription scope.'
    policyDefinitionId: appServiceAuthInit.id
    enforcementMode: 'Default'
    parameters: {
      effect: { value: effect }
    }
  }
}

output initiativeId string = appServiceAuthInit.id
output assignmentId string = appServiceAuthInitAssignment.id
