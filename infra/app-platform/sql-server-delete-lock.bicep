/*
  SQL Server CanNotDelete Lock

  Purpose:
  - Grants CanNotDelete lock to SQL Server.

  Scope:
  - SQL Server
*/

targetScope = 'resourceGroup'

@allowed(['CanNotDelete','ReadOnly'])
param lockLevel string = 'CanNotDelete'

param lockName string
param sqlServerName string

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' existing = {
  name: sqlServerName
}

resource sqlLock 'Microsoft.Authorization/locks@2016-09-01' = {
  name: lockName
  scope: sqlServer
  properties: {
    level: lockLevel
    notes: 'Prevent accidental deletion of the SQL Server.'
  }
}
