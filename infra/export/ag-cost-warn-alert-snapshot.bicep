param actionGroups_ag_cost_warn_lab_name string = 'ag-cost-warn-lab'

resource actionGroups_ag_cost_warn_lab_name_resource 'microsoft.insights/actionGroups@2024-10-01-preview' = {
  name: actionGroups_ag_cost_warn_lab_name
  location: 'Global'
  tags: {
    workload: 'iphub'
    env: 'lab'
    purpose: 'alerts'
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
