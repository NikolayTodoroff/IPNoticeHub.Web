/*
  Subscription Budget Definition

  Purpose:
   - Establishes a monthly cost control boundary at the subscription level to prevent budget overruns.
   - Provides multi-tiered alerting (Informational, Warning, Critical) through Action Groups.
   - Monitors both actual expenditure and forecasted spending trends to enable proactive cost management.
   
   - Actual spending thresholds: 25%, 50%, 75%, 100% of monthly budget
   - Forecasted spending thresholds: 30%, 60%, 90% of monthly budget
   - Direct email notifications to budget owners
   - Integration with centralized Action Groups for escalated responses

  Scope:
  - Subscription
*/

targetScope = 'subscription'

param budgetName string = 'bud-iphub-sub-lab'
param budgetAmount int = 100
param startDate string = '2026-01-01T00:00:00Z'
param endDate string = '2027-12-31T00:00:00Z'

param actionGroupInfoId string
param actionGroupWarnId string
param actionGroupCritId string

param contactEmails array

resource budget 'Microsoft.Consumption/budgets@2024-08-01' = {
  name: budgetName
  properties: {
    category: 'Cost'
    amount: budgetAmount
    timeGrain: 'Monthly'
    
    timePeriod: {
      startDate: startDate
      endDate: endDate
    }
    notifications: {
      actual_GreaterThan_25_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 25
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ actionGroupInfoId ]
      }
      actual_GreaterThan_50_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 50
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ actionGroupInfoId ]
      }
      actual_GreaterThan_75_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 75
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ actionGroupWarnId ]
      }
      actual_GreaterThan_100_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ actionGroupCritId ]
      }
      forecasted_GreaterThan_30_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 30
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ actionGroupInfoId ]
      }
      forecasted_GreaterThan_60_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 60
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ actionGroupWarnId ]
      }
      forecasted_GreaterThan_90_Percent: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 90
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ actionGroupCritId ]
      }
    }
  }
}

