// ==============================================================================
// Project: IPNoticeHub
// Purpose: Securely store SQL Connection String in Key Vault
// Security: Uses @secure() to prevent secret leakage in deployment logs.
// ==============================================================================

@description('The name of the pre-existing Key Vault. Scoped as "existing" below.')
param keyVaultName string

@description('The SQL connection string. This is marked @secure so it is never logged.')
@secure()
param sqlConnectionString string

// --- REFERENCE EXISTING VAULT ---

resource kv 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyVaultName
}

// --- CREATE SECRET ---
resource sqlConnSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: kv
  name: 'sql-connectionstring'
  properties: {
    value: sqlConnectionString
    
    contentType: 'SQL connection string (Managed Identity)'
    
    attributes: {
      enabled: true
    }
  }
}

@description('The URI of the secret.')
output secretUri string = sqlConnSecret.properties.secretUri
