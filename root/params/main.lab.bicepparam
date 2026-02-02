using '../infra/main.bicep'

param location = 'westeurope'
param alertEmail = 'everflowing555@gmail.com'

param env = 'lab'
param owner = 'nikolay'
param region = 'weu'
param workload = 'ipnoticehub'

param breakGlassUpn = 'emergency.admin@everflowing555gmail.onmicrosoft.com'
param globalAdminUpn = 'nikolay.todorov@ipnoticehub.com'
param sqlAdminUpn = 'sql.admin@ipnoticehub.com'
param testUserUpn = 'test.user@ipnoticehub.com'

param adminSecurityGroupName = 'IPNoticeHub-App-Admins'
param userSecurityGroupName = 'IPNoticeHub-App-Users'

param keyVaultName = 'kv-ipnoticehub-dev-weu'
param sqlDatabaseName = 'ipnoticehub-db'
param sqlServerName = 'sql-ipnoticehub-dev'
param storageAccountName = 'stipnoticehubdocslabweu'
param appServiceName = 'ipnoticehub-web-lab'

param dbPrivateDnsName = 'privatelink.database.windows.net'
param kvPrivateDnsName = 'privatelink.vaultcore.azure.net'
param blobPrivateDnsName = 'privatelink.blob.core.windows.net'

param policyRemediationUamiResourceId = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourcegroups/rg-ipnoticehub-lab-weu/providers/Microsoft.ManagedIdentity/userAssignedIdentities/uami-iphub-policy-remediation'
param globalAdminObjectId = '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'



