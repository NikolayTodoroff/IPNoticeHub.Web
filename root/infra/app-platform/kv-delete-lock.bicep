/*
  Key Vault CanNotDelete Lock

  Purpose:
  - Grants CanNotDelete lock to Key Vault.

  Scope:
  - Key Vault
*/

targetScope = 'resourceGroup'

@allowed(['CanNotDelete','ReadOnly'])
param lockLevel string = 'CanNotDelete'

param lockName string
param keyVaultName string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource kvLock 'Microsoft.Authorization/locks@2016-09-01' = {
  name: lockName
  scope: keyVault
  properties: {
    level: lockLevel
    notes: 'Prevent accidental deletion of Key Vault.'
  }
}
