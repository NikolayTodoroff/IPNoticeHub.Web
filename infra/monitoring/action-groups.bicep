/* RESOURCE MODULE: Azure Monitor Action Groups (Cost Alerting)
   PROJECT: IPNoticeHub
   DATE: 2026-01-14
   
   DESCRIPTION: 
   This module sets up notification channels (Action Groups) used for budget alerts.
   It creates three distinct tiers: Info, Warning, and Critical.
   
   COMPLIANCE: 
   Ensures that cost-management emails are sent to the designated owner to prevent 
   unexpected Azure spending ("Cloud Shock").
*/

// --- PARAMETERS ---

@description('Workload/application short name (e.g., iphub)')
param workload string = 'iphub'

@description('Environment (e.g., lab, dev, prod)')
param env string = 'lab'

@description('Owner tag value for cost tracking and accountability')
param owner string = 'nikolay'

@description('Primary email recipient for cost alerts - Must be a valid email format')
param alertEmail string = 'everflowing555@gmail.com'

// --- VARIABLES ---

@description('Action Groups are global resources but require a location metadata value')
var location = 'Global'

@description('Standardized tagging schema for cost center allocation')
var commonTags = {
  workload: workload
  env: env
  purpose: 'cost-alerting'
  owner: owner
}

@description('Definition of alert tiers. Adding a level here automatically generates a new Action Group.')
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

// --- RESOURCES ---

@description('Iterative deployment of Action Groups based on the actionGroups array definition')
resource ag 'Microsoft.Insights/actionGroups@2023-01-01' = [for agDef in actionGroups: {
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
}]

output actionGroupIds object = {
  info: ag[0].id
  warn: ag[1].id
  crit: ag[2].id
}