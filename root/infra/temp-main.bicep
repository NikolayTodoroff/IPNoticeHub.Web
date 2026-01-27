targetScope = 'subscription'

module appServiceDiagPolicy './governance/app-service-diag-settings-policy.bicep' = {
  name: 'appServiceDiagPolicy'
  scope: subscription()
  params: {
    logAnalytics: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourcegroups/rg-ipnoticehub-lab-weu/providers/microsoft.operationalinsights/workspaces/log-ipnoticehub-lab-weu'
  }
}

module coreSvsMonitoring './governance/core-services-monitoring-init.bicep' = {
  name: 'coreSvsMonitoring'
  scope: subscription()
  params: {
    location: 'westeurope'
    policyRemediationUamiResourceId: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourcegroups/rg-ipnoticehub-lab-weu/providers/Microsoft.ManagedIdentity/userAssignedIdentities/uami-iphub-policy-remediation'
    initiativeName: 'core-svs-monitoring-init-iphub-lab-weu'
    assignmentName: 'assign-core-svs-monitoring-init-iphub-lab-weu'
    logAnalytics: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourcegroups/rg-ipnoticehub-lab-weu/providers/microsoft.operationalinsights/workspaces/log-ipnoticehub-lab-weu'
    appServicePolicyDefinitionId: appServiceDiagPolicy.outputs.appServicePolicyId
  }
}
