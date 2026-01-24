using '../infra/governance/governance-main.bicep'

param contactEmail = 'everflowing555@gmail.com'
param location = 'westeurope'

param actionGroupInfoName = 'ag-cost-info-lab'
param actionGroupWarnName = 'ag-cost-warn-lab'
param actionGroupCritName = 'ag-cost-crit-lab'
param alertsRgName = 'rg-alerts-iphub-lab-weu'

param env = 'lab'
param owner = 'nikolay'
param region = 'weu'
param workload = 'ipnoticehub'
