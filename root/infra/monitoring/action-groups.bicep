/*
  Cost Alerting Action Groups

  Purpose:
  - Establishes tiered notification channels (Action Groups) for Azure cost budget alerts.
  - Creates three severity levels: Informational, Warning, and Critical.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param alertEmail string = 'everflowing555@gmail.com'
param tags object = {}

var location = 'Global'

var actionGroups = [
  {
    name: 'ag-cost-info-lab'
    shortName: 'AzCost-Info'
    level: 'info'
  }
  {
    name: 'ag-cost-warn-lab'
    shortName: 'AzCost-Warn'
    level: 'warn'
  }
  {
    name: 'ag-cost-crit-lab'
    shortName: 'AzCost-Crit'
    level: 'crit'
  }
]

resource ag 'Microsoft.Insights/actionGroups@2023-01-01' = [for agDef in actionGroups: {
  name: agDef.name
  location: location
  tags: union(tags, { purpose: 'alerts' })
  properties: {
    groupShortName: agDef.shortName
    enabled: true
    emailReceivers: [
      {
        name: 'notify-email-cost-${agDef.level}-lab-_EmailAction-'
        emailAddress: alertEmail
        useCommonAlertSchema: false
      }
    ]
  }
}]

output actionGroupIds object = {
  info: ag[0].id
  warn: ag[1].id
  crit: ag[2].id
}

