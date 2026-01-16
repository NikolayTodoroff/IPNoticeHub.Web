/* SCOPE: Resource Group
   PROJECT: IPNoticeHub
   DESCRIPTION: Central security vault for managing application secrets and keys.
*/

@description('The name of the Key Vault')
param vaults_kv_name string = 'kv-ipnoticehub-dev-weu'

@description('The location for the resource')
param location string = 'westeurope'

// --- KEY VAULT DEFINITION ---

resource keyVault 'Microsoft.KeyVault/vaults@2024-12-01-preview' = {
  name: vaults_kv_name
  location: location
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
  }
  properties: {
    sku: {
      family: 'A'
      name: 'Standard'
    }
    tenantId: '227c0e16-85bf-4c81-a620-0ce328414830'
    
    // --- NETWORK SECURITY ---
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    
    // --- ACCESS & AUTHORIZATION ---
    accessPolicies: [] // Empty because we are using RBAC (Best Practice)
    enableRbacAuthorization: true // Modern authorization model [Az500 standard]
    
    // --- PROTECTION & RECOVERY ---
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true // Prevents permanent deletion during retention
    
    // --- INTEGRATION SETTINGS ---
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    
    publicNetworkAccess: 'Enabled'
    vaultUri: 'https://${vaults_kv_name}.vault.azure.net/'
  }
}

output kvId string = keyVault.id
output kvUri string = keyVault.properties.vaultUri