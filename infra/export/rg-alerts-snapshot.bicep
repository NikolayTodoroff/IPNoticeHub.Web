param actionGroups_ag_cost_crit_lab_name string = 'ag-cost-crit-lab'
param actionGroups_ag_cost_info_lab_name string = 'ag-cost-info-lab'
param actionGroups_ag_cost_warn_lab_name string = 'ag-cost-warn-lab'

resource actionGroups_ag_cost_crit_lab_name_resource 'microsoft.insights/actionGroups@2024-10-01-preview' = {
  name: actionGroups_ag_cost_crit_lab_name
  location: 'Global'
  tags: {
    workload: 'iphub'
    env: 'lab'
    purpose: 'cost-alerting'
    owner: 'nikolay'
    region: 'weu'
  }
  properties: {
    groupShortName: 'AzCost-Crit'
    enabled: true
    emailReceivers: [
      {
        name: 'notify-email-cost-crit-lab_-EmailAction-'
        emailAddress: 'everflowing555@gmail.com'
        useCommonAlertSchema: false
      }
    ]
    smsReceivers: []
    webhookReceivers: []
    eventHubReceivers: []
    itsmReceivers: []
    azureAppPushReceivers: []
    automationRunbookReceivers: []
    voiceReceivers: []
    logicAppReceivers: []
    azureFunctionReceivers: []
    armRoleReceivers: []
  }
}

resource actionGroups_ag_cost_info_lab_name_resource 'microsoft.insights/actionGroups@2024-10-01-preview' = {
  name: actionGroups_ag_cost_info_lab_name
  location: 'Global'
  tags: {
    workload: 'iphub'
    env: 'lab'
    purpose: 'cost-alerting'
    owner: 'nikolay'
    region: 'weu'
  }
  properties: {
    groupShortName: 'AzCost-Info'
    enabled: true
    emailReceivers: [
      {
        name: 'notify-email-cost-info-lab_-EmailAction-'
        emailAddress: 'everflowing555@gmail.com'
        useCommonAlertSchema: false
      }
    ]
    smsReceivers: []
    webhookReceivers: []
    eventHubReceivers: []
    itsmReceivers: []
    azureAppPushReceivers: []
    automationRunbookReceivers: []
    voiceReceivers: []
    logicAppReceivers: []
    azureFunctionReceivers: []
    armRoleReceivers: []
  }
}

resource actionGroups_ag_cost_warn_lab_name_resource 'microsoft.insights/actionGroups@2024-10-01-preview' = {
  name: actionGroups_ag_cost_warn_lab_name
  location: 'Global'
  tags: {
    workload: 'iphub'
    env: 'lab'
    purpose: 'cost-alerting'
    owner: 'nikolay'
    region: 'weu'
  }
  properties: {
    groupShortName: 'AzCost-Warn'
    enabled: true
    emailReceivers: [
      {
        name: 'notify-email-cost-warn-lab_-EmailAction-'
        emailAddress: 'everflowing555@gmail.com'
        useCommonAlertSchema: false
      }
    ]
    smsReceivers: []
    webhookReceivers: []
    eventHubReceivers: []
    itsmReceivers: []
    azureAppPushReceivers: []
    automationRunbookReceivers: []
    voiceReceivers: []
    logicAppReceivers: []
    azureFunctionReceivers: []
    armRoleReceivers: []
  }
}
