using '../infra/network/network.bicep'

param env = 'lab'
param owner = 'nikolay'
param region = 'weu'
param workload = 'ipnoticehub'

param location = 'westeurope'

param appSvcSubnetName = 'snet-appsvc-ipnoticehub-lab-weu'
param peSubnetName = 'snet-pe-ipnoticehub-lab-weu'
param mgmtSubnetName = 'snet-mgmt-ipnoticehub-lab-weu'

param dbPrivateDnsName = 'privatelink.database.windows.net'
param kvPrivateDnsName = 'privatelink.vaultcore.azure.net'
param blobPrivateDnsName = 'privatelink.blob.core.windows.net'

param sqlServerResourceId = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourceGroups/rg-ipnoticehub-lab-weu/providers/Microsoft.Sql/servers/sql-ipnoticehub-dev'
param keyVaultResourceId = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourceGroups/rg-ipnoticehub-lab-weu/providers/Microsoft.KeyVault/vaults/kv-ipnoticehub-dev-weu'
param storageAccountResourceId = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourceGroups/rg-ipnoticehub-lab-weu/providers/Microsoft.Storage/storageAccounts/stipnoticehubdocslabweu'
