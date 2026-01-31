/*
  App Service Local Authentication Methods Disabled Configuration

  Purpose:
  - Disables local authentication methods for both FTP and SCM on App Services for security.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('App Service name to apply the diagnostic settings to.')
param appServiceName string

resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

resource ftpCreds 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: appService
  name: 'ftp'
  properties: {
    allow: false
  }
}

resource scmCreds 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: appService
  name: 'scm'
  properties: {
    allow: false
  }
}
