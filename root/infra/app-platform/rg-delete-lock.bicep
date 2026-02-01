/*
  Resource Group CanNotDelete Lock

  Purpose:
  - Grants CanNotDelete lock to Resource Group.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@allowed(['CanNotDelete','ReadOnly'])
param lockLevel string = 'CanNotDelete'

@description('Lock name.')
param lockName string

resource rgNetwork 'Microsoft.Authorization/locks@2020-05-01' = {
  name: lockName
  properties: {
    level: lockLevel
    notes: 'Lock to prevent accidental deletion of a Resource Group.'
  }
}
