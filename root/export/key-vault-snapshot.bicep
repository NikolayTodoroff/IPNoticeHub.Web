param vaults_kv_ipnoticehub_dev_weu_name string = 'kv-ipnoticehub-dev-weu'

resource vaults_kv_ipnoticehub_dev_weu_name_resource 'Microsoft.KeyVault/vaults@2024-12-01-preview' = {
  name: vaults_kv_ipnoticehub_dev_weu_name
  location: 'westeurope'
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
    networkAcls: {
      bypass: 'None'
      defaultAction: 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
    accessPolicies: []
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
    enablePurgeProtection: true
    vaultUri: 'https://${vaults_kv_ipnoticehub_dev_weu_name}.vault.azure.net/'
    provisioningState: 'Succeeded'
    publicNetworkAccess: 'Enabled'
  }
}

resource vaults_kv_ipnoticehub_dev_weu_name_sql_connectionstring_lab 'Microsoft.KeyVault/vaults/secrets@2024-12-01-preview' = {
  parent: vaults_kv_ipnoticehub_dev_weu_name_resource
  name: 'sql-connectionstring-lab'
  location: 'westeurope'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    owner: 'nikolay'
    region: 'weu'
  }
  properties: {
    attributes: {
      enabled: true
    }
  }
}
