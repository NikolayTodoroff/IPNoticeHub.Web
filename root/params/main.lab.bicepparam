using '../infra/main.bicep'

param location = 'westeurope'
param alertEmail = 'everflowing555@gmail.com'

param env = 'lab'
param owner = 'nikolay'
param region = 'weu'
param workload = 'ipnoticehub'

param keyVaultName = 'kv-ipnoticehub-dev-weu'
param sqlDatabaseName = 'ipnoticehub-db'
param sqlServerName = 'sql-ipnoticehub-dev'
param storageAccountName = 'stipnoticehubdocslabweu'

param policyRemediationUamiResourceId = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourcegroups/rg-ipnoticehub-lab-weu/providers/Microsoft.ManagedIdentity/userAssignedIdentities/uami-iphub-policy-remediation'
param sqlMonitoringAssignmentName = 'assign-sql-monitoring-init-iphub-lab-weu'
param sqlMonitoringInitiativeName = 'sql-monitoring-init-iphub-lab-weu'
