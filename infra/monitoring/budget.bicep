/* RESOURCE MODULE: Azure Subscription Budget
   PROJECT: IPNoticeHub
   DATE: 2026-01-14
   
   DESCRIPTION: 
   This template establishes a monthly financial guardrail at the subscription scope. 
   It triggers tiered notifications (Info, Warning, Critical) based on both 
   Actual spending and Forecasted trends.
   
   GOVERNANCE: 
   Aligned with the 'Cost Optimization' pillar of the Azure Well-Architected Framework.
*/

targetScope = 'subscription'

// --- PARAMETERS ---

@description('Budget resource name - unique at the subscription level.')
param budgetName string = 'bud-iphub-sub-lab'

@description('Monthly budget ceiling in local currency. Alerts are calculated as a % of this.')
param budgetAmount int = 100

@description('Budget validity start date (YYYY-MM-DD). Must be the first of the month.')
param startDate string = '2026-01-01'

@description('Budget expiration date (YYYY-MM-DD).')
param endDate string = '2027-12-31'

@description('List of email addresses to receive direct notification outside of Action Groups.')
param contactEmails array = [
  'everflowing555@gmail.com'
]

@description('The Resource Group where the shared Action Groups are deployed.')
param alertsRgName string = 'rg-alerts-iphub-lab-weu'

// --- EXISTING RESOURCE REFERENCES ---

@description('Reference to the Resource Group containing the Action Groups for scope bridging.')
resource alertsRg 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: alertsRgName
}

// References to the Action Groups created in the previous module
resource agInfo 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  scope: alertsRg
  name: 'ag-cost-info-lab'
}

resource agWarn 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  scope: alertsRg
  name: 'ag-cost-warn-lab'
}

resource agCrit 'Microsoft.Insights/actionGroups@2023-01-01' existing = {
  scope: alertsRg
  name: 'ag-cost-crit-lab'
}

// --- BUDGET DEFINITION ---

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
      // --- ACTUAL COST THRESHOLDS ---
      // Triggers when the real-time spend exceeds the specified percentage.

      actual25: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 25
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ agInfo.id ] // Mapped to Info tier
      }
      actual50: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 50
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ agInfo.id ] 
      }
      actual75: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 75
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ agWarn.id ] // Escalation to Warning tier
      }
      actual100: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [ agCrit.id ] // Escalation to Critical tier
      }

      // --- FORECASTED COST THRESHOLDS ---
      // Predictive alerts based on current usage patterns to prevent end-of-month surprises.

      forecast30: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 30
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ agInfo.id ]
      }
      forecast60: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 60
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ agWarn.id ]
      }
      forecast90: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 90
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [ agCrit.id ]
      }
    }
  }
}
