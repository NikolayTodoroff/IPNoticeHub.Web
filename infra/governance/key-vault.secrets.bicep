/*
  Key Vault Connection String Secret Deployment

  Purpose:
  - Securely store SQL Connection String in Key Vault
  - Uses @secure() to prevent secret leakage in deployment logs.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param keyVaultName string

@secure()
param sqlConnectionString string

param secretName string = 'sql-connectionstring-lab'

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

resource sqlConnSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: secretName
  properties: {
    value: sqlConnectionString
    contentType: 'SQL connection string (Managed Identity)'
    attributes: {
      enabled: true
    }
  }
}

output secretUri string = sqlConnSecret.properties.secretUri
output secretNameOut string = sqlConnSecret.name

