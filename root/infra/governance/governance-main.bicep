targetScope = 'subscription'

var stAccSkuPolicyAssignName = 'assign-storage-allowed-skus-${workload}-${env}-${region}'
var allowedLocationsPolicyAssignName = 'assign-allowed-locations-${workload}-${env}-${region}'
var taggingInitAssignName = 'assign-tagging-governance-${workload}-${env}-${region}'
var taggingEnforceInitName = 'init-tagging-governance'

var appServiceHttpsPolAssignName = 'assign-appservice-https-only-${workload}-${env}-${region}'
var stHttpsAssignPolName = 'assign-storage-https-only-${workload}-${env}-${region}'
var sqlTlsAssignPolName = 'assign-sql-min-tls-${workload}-${env}-${region}'

var actionGroupInfoId = resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', actionGroupInfoName)
var actionGroupWarnId = resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', actionGroupWarnName)
var actionGroupCritId = resourceId(subscription().subscriptionId, alertsRgName, 'Microsoft.Insights/actionGroups', actionGroupCritName)

param alertsRgName string
param actionGroupInfoName string
param actionGroupWarnName string
param actionGroupCritName string

param contactEmail string
param location string

param workload string
param env string
param region string
param owner string

module budget './budget.bicep' = {
  name: 'budget'
  scope: subscription()
  params: {
    budgetName: 'bud-iphub-sub-lab'
    budgetAmount: 100
    startDate: '2026-01-01T00:00:00Z'
    endDate: '2027-12-31T00:00:00Z'
    contactEmails: [ contactEmail ]

    actionGroupInfoId: actionGroupInfoId
    actionGroupWarnId: actionGroupWarnId
    actionGroupCritId: actionGroupCritId
  }
}

module storageAccSkuPolicy './storage-acc-sku-policy.bicep' = {
  name: 'storageAccSkuPolicy'
  scope: subscription()
  params: {
    assignmentName: stAccSkuPolicyAssignName
  }
}

module allowedLocationsPolicy './allowed-locations-policy.bicep' = {
  name: 'allowedLocationsPolicy'
  scope: subscription()
  params: {
    assignmentName: allowedLocationsPolicyAssignName
  }
}

module taggingGovernanceInit './tagging-governance-init.bicep' = {
  name: 'taggingGovernanceInit'
  scope: subscription()
  params: {
    assignmentName: taggingInitAssignName
    initiativeName: taggingEnforceInitName
    location: location

    workload: workload
    env: env
    region: region
    owner: owner
  }
}

module appServiceHttpsOnly './appservice-https-only-policy.bicep' = {
  name: 'appServiceHttpsOnly'
  scope: subscription()
  params: {
    assignmentName: appServiceHttpsPolAssignName
    effect: 'Audit'
  }
}

module storageHttpsOnly './storage-https-only-policy.bicep' = {
  name: 'storageHttpsOnly'
  scope: subscription()
  params: {
    assignmentName: stHttpsAssignPolName
  }
}

module sqlMinTls './sql-min-tls-policy.bicep' = {
  name: 'sqlMinTls'
  scope: subscription()
  params: {
    assignmentName: sqlTlsAssignPolName
    effect: 'Audit'
    minTlsVersion: '1.2'
  }
}
