/*
  SQL Admin Configuration

  Purpose:
  - Sets up an Entra ID admin for the SQL Server and enforces Entra ID-only authentication

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param serverName string
param entraAdminLogin string
param entraAdminObjectId string

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' existing = {
  name: serverName
}

resource sqlAdmin 'Microsoft.Sql/servers/administrators@2023-05-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: entraAdminLogin
    sid: entraAdminObjectId
    tenantId: subscription().tenantId
  }
}

resource entraOnly 'Microsoft.Sql/servers/azureADOnlyAuthentications@2023-05-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
}


