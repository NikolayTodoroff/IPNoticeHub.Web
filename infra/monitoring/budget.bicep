targetScope = 'subscription'

@description('Budget resource name.')
param budgetName string = 'bud-iphub-sub-lab'

@description('Monthly budget amount.')
param budgetAmount int = 100

@description('Budget start date (YYYY-MM-DD).')
param startDate string = '2026-01-01'

@description('Budget end date (YYYY-MM-DD).')
param endDate string = '2027-12-31'

@description('Email recipients.')
param contactEmails array = [
  'everflowing555@gmail.com'
]

@description('Resource group where Action Groups live.')
param alertsRgName string = 'rg-alerts-iphub-lab-weu-01'

// --- Existing Action Groups (created by action-groups.bicep) ---
resource alertsRg 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: alertsRgName
}

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

// --- Budget ---
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
      // ---- ACTUAL COST ----
      actual25: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 25
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [
          agInfo.id
        ]
      }
      actual50: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 50
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [
          agInfo.id
        ]
      }
      actual75: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 75
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [
          agWarn.id
        ]
      }
      actual100: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Actual'
        contactEmails: contactEmails
        contactGroups: [
          agCrit.id
        ]
      }

      // ---- FORECASTED COST ----
      forecast30: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 30
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [
          agInfo.id
        ]
      }
      forecast60: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 60
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [
          agWarn.id
        ]
      }
      forecast90: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 90
        thresholdType: 'Forecasted'
        contactEmails: contactEmails
        contactGroups: [
          agCrit.id
        ]
      }
    }
  }
}
