@description('Workload/application short name (e.g., iphub)')
param workload string = 'iphub'

@description('Environment (e.g., lab, dev, prod)')
param env string = 'lab'

@description('Owner tag value')
param owner string = 'nikolay'

@description('Primary email recipient for cost alerts')
param alertEmail string = 'everflowing555@gmail.com'

var location = 'Global'

var commonTags = {
  workload: workload
  env: env
  purpose: 'cost-alerting'
  owner: owner
}

var actionGroups = [
  {
    name: 'ag-cost-info-${env}'
    shortName: 'AzCost-Info'
    level: 'info'
  }
  {
    name: 'ag-cost-warn-${env}'
    shortName: 'AzCost-Warn'
    level: 'warn'
  }
  {
    name: 'ag-cost-crit-${env}'
    shortName: 'AzCost-Crit'
    level: 'crit'
  }
]

resource ag 'Microsoft.Insights/actionGroups@2023-01-01' = [
  for agDef in actionGroups: {
    name: agDef.name
    location: location
    tags: commonTags
    properties: {
      groupShortName: agDef.shortName
      enabled: true
      emailReceivers: [
        {
          name: 'notify-email-cost-${agDef.level}-${env}'
          emailAddress: alertEmail
          useCommonAlertSchema: false
        }
      ]
    }
  }
]

output actionGroupIds object = {
  info: ag[0].id
  warn: ag[1].id
  crit: ag[2].id
}
