var suffix = '${workload}-${env}-${region}'

var gov = {
  monitoring: {
    sqlMonitoringInitiativeName: 'init-sql-monitoring-${suffix}'
    sqlMonitoringAssignmentName: 'assign-sql-monitoring-${suffix}'
    coreSvsMonitoringInitiativeName: 'init-core-monitoring-${suffix}'
    coreSvsMonitoringAssignmentName: 'assign-core-monitoring-${suffix}'
  }
  tags: {   
    taggingInitAssignName: 'assign-tagging-governance-${suffix}'
    appServiceAuthInitAssignName: 'assign-appservice-auth-init-${suffix}'
    appServiceTlsAssignName: 'assign-appservice-tls-${suffix}'
  }
  actionGroups: {
    actionGroupWarnName: 'ag-warn-${suffix}'
    actionGroupInfoName: 'ag-info-${suffix}'
    actionGroupCritName: 'ag-crit-${suffix}'
  }
  assignments: {
    storageAccSkuPolicyAssignName: 'assign-storage-allowed-skus-${suffix}'
    allowedLocationsPolicyAssignName: 'assign-allowed-locations-${suffix}'
    appServiceHttpsPolAssignName: 'assign-appservice-https-only-${suffix}'
    storageHttpsAssignPolName: 'assign-storage-https-only-${suffix}'
    sqlTlsAssignPolName: 'assign-sql-min-tls-${suffix}'
    appServiceManagedIdentityPolicyAssignName: 'assign-appservice-managed-identity-${suffix}'
    appServiceTlsAssignName: 'assign-appservice-tls-${suffix}'
    keyVaultRbacAssignName: 'assign-kv-rbac-${suffix}'
    appServiceAuthInitAssignName: 'assign-appservice-auth-init-${suffix}'
    storageAccMonitoringAssignmentName: 'assign-storage-monitoring-init-${suffix}'
    storageAccMonitoringInitiativeName: 'storage-monitoring-init-${suffix}'
    sqlDbTlsAssignName: 'assign-sql-db-tls-audit-${suffix}'
    privateEndpointOnlyInitiativeName: 'init-private-endpoint-only-${suffix}'
    privateEndpointOnlyAssignmentName: 'assign-private-endpoint-only-${suffix}'
    blobsLifecycleMngmtPolAssignName: 'assign-blobs-lifecycle-management-${suffix}'
    blobsVersioningAssignmentName: 'assign-blobs-versioning-${suffix}'
  }
}

var actionGroupIds = {
  warn: resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', gov.actionGroups.actionGroupWarnName)
  info: resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', gov.actionGroups.actionGroupInfoName)
  crit: resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', gov.actionGroups.actionGroupCritName)
}

param alertsRgName string
param mainRgName string
param appServiceName string
param logAnalyticsWorkspaceId string
param policyRemediationUamiResourceId string
param keyVaultName string
param storageAccountName string

param contactEmail string
param location string

param workload string
param env string
param region string
param owner string

module budget './budgets/subscription-budget-core.bicep' = {
  name: 'budget'
  scope: subscription()
  params: {
    budgetName: 'bud-iphub-sub-lab'
    budgetAmount: 100
    startDate: '2026-01-01T00:00:00Z'
    endDate: '2027-12-31T00:00:00Z'
    contactEmails: [ contactEmail ]

    actionGroupInfoId: actionGroupIds.info
    actionGroupWarnId: actionGroupIds.warn
    actionGroupCritId: actionGroupIds.crit
  }
}

module storageAccSkuPolicy './policyAssignments/storage-acc-sku-policy-assign.bicep' = {
  name: 'storageAccSkuPolicy'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.storageAccSkuPolicyAssignName
  }
}

module allowedLocationsPolicy './policyAssignments/allowed-locations-policy-assign.bicep' = {
  name: 'allowedLocationsPolicy'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.allowedLocationsPolicyAssignName
  }
}

module taggingGovernanceInit './initiatives/tagging-governance-init.bicep' = {
  name: 'taggingGovernanceInit'
  scope: subscription()
  params: {
    assignmentName: gov.tags.taggingInitAssignName
    initiativeName: 'init-tagging-governance'
    location: location

    workload: workload
    env: env
    region: region
    owner: owner
  }
}

module appServiceHttpsOnlyPolicy './policyAssignments/app-service-https-modify-policy-assign.bicep' = {
  name: 'appServiceHttpsOnly'
  scope: subscription()
  params: {
    location: location
    assignmentName: gov.assignments.appServiceHttpsPolAssignName
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    effect: 'Modify'
  }
}

module storageHttpsOnlyPolicy './policyAssignments/storage-https-only-policy-assign.bicep' = {
  name: 'storageHttpsOnly'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.storageHttpsAssignPolName
  }
}

module sqlMinTlsPolicy './policyAssignments/sql-server-min-tls-policy-assign.bicep' = {
  name: 'sqlMinTls'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.sqlTlsAssignPolName
    effect: 'Audit'
    minTlsVersion: '1.2'
  }
}

module appServiceTlsPolicy './policyAssignments/app-service-tls-policy-assign.bicep' = {
  name: 'appServiceTls'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.appServiceTlsAssignName
    effect: 'AuditIfNotExists'
  }
}

module sqlMonitoringInit './initiatives/sql-monitoring-init.bicep' = {
  name: 'sqlMonitoring'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    initiativeName: gov.monitoring.sqlMonitoringInitiativeName
    assignmentName: gov.monitoring.sqlMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspaceId
  }
}

module appServiceManagedIdentityPolicy './policyAssignments/app-service-managed-identity-policy-assign.bicep' = {
  name: 'appServiceManagedIdentity'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.appServiceManagedIdentityPolicyAssignName
    effect: 'AuditIfNotExists'
  }
}

module keyVaultRbacPolicy './policyAssignments/kv-rbac-policy-assign.bicep' = {
  name: 'keyVaultRbac'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.keyVaultRbacAssignName
    effect: 'Audit'
  }
}

module appServiceAuthConfig './config/app-service-auth-disabled-config.bicep' = {
  name: 'appServiceAuthConfig'
  scope: resourceGroup(mainRgName)
  params: {
    appServiceName: appServiceName
  }
}

module appServiceAuthInit './initiatives/app-service-auth-init.bicep' = {
  name: 'appServiceAuthInit'
  scope: subscription()
  params: {
    location: location
    initiativeName: 'app-service-auth-init-initiative'
    assignmentName: gov.assignments.appServiceAuthInitAssignName
    effect: 'AuditIfNotExists'
  }
}

module appServiceDiagPolicy './policyDefinitions/app-service-diag-settings-policy-def.bicep' = {
  name: 'appServiceDiagPolicy'
  scope: subscription()
  params: {
    logAnalytics: logAnalyticsWorkspaceId
  }
}

module coreSvsMonitoringInit './initiatives/core-services-monitoring-init.bicep' = {
  name: 'coreSvsMonitoringInit'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    initiativeName: gov.monitoring.coreSvsMonitoringInitiativeName
    assignmentName: gov.monitoring.coreSvsMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspaceId
    appServicePolicyDefinitionId: appServiceDiagPolicy.outputs.appServicePolicyId
  }
}

module storageAccMonitoring './initiatives/storage-acc-monitoring-init.bicep' = {
  name: 'storageAccMonitoring'
  scope: subscription()
  params: {
    location: location
    initiativeName: gov.assignments.storageAccMonitoringInitiativeName
    assignmentName: gov.assignments.storageAccMonitoringAssignmentName
    logAnalytics: logAnalyticsWorkspaceId
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
  }
}

module sqDbTlsPolicy './policyAssignments/sql-db-tls-policy-assign.bicep' = {
  name: 'sqlDbTls'
  scope: subscription()
  params: {
    assignmentName: gov.assignments.sqlDbTlsAssignName
    effect: 'Audit'
  }
}

module privateEndpointOnlyInit './initiatives/private-endpoint-only-init.bicep' = {
  name: 'privateEndpointOnlyInit'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    initiativeName: gov.assignments.privateEndpointOnlyInitiativeName
    assignmentName: gov.assignments.privateEndpointOnlyAssignmentName
    effectModify: 'Modify'
    effectAudit: 'Audit'
  }
}

module kvPublicAccessDisabled './config/kv-public-access-disabled-config.bicep' = {
  name: 'kvPublicAccessDisabled'
  scope: resourceGroup(mainRgName)
  params: {
    location: location
    keyVaultName: keyVaultName
  }
}

module blobsLifecycleManagementPolicy './policyAssignments/blobs-lifecycle-management-policy-assign.bicep' = {
  name: 'blobsLifecycleManagementPolicy'
  scope: resourceGroup(mainRgName)
  params: {
    storageAccountName: storageAccountName
  }
}

module blobsVersioningPolicy './policyAssignments/blobs-versioning-policy-assign.bicep' = {
  name: 'blobsVersioningPolicy'
  scope: subscription()
  params: {
    location: location
    policyRemediationUamiResourceId: policyRemediationUamiResourceId
    assignmentName: gov.assignments.blobsVersioningAssignmentName
    effect: 'Modify'
  }
}
