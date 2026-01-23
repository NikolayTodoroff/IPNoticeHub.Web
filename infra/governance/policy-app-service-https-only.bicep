/*
  Azure Policy Assignment – App Service apps accessible only over HTTPS

  Purpose:
   - Restricts Azure App Service apps to be accessible only via HTTPS to enhance
  security and data protection.
   - Effect: Deny

  Scope:
  Subscription
*/

targetScope = 'subscription'

param assignmentName string = 'assign-app-service-https-only-iphub'

@allowed([
  'Audit'
  'Deny'
  'Disabled'
])
param effect string = 'Audit'

resource appServiceHttpsOnly 'Microsoft.Authorization/policyAssignments@2025-03-01' = {
  name: assignmentName
  properties: {
    displayName: 'App Service apps should only be accessible over HTTPS'
    description: 'Audits (or denies) App Service apps that do not enforce HTTPS-only (httpsOnly=true).'
    policyDefinitionId: '/providers/Microsoft.Authorization/policyDefinitions/a4af4a39-4135-47fb-b175-47fbdf85311d'
    enforcementMode: 'Default'
    parameters: {
      effect: {
        value: effect
      }
    }
    nonComplianceMessages: [
      {
        message: 'HTTPS-only is not enforced for this App Service app (httpsOnly=false). Enable “HTTPS Only” in TLS/SSL settings to protect data in transit.'
      }
    ]
  }
}

