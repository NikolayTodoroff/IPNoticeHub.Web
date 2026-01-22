/*
  SQL Server Deployment

  Purpose:
  Deploys a SQL Server logical instance and database with Entra ID-only authentication,
  Microsoft Defender for SQL, and Express Vulnerability Assessment for security compliance.

  Key features:
  - Entra ID-only authentication (no SQL logins)
  - TLS 1.2 minimum encryption
  - Microsoft Defender for SQL (Advanced Threat Protection)
  - Express Vulnerability Assessment (no storage account required)
  - Firewall rules for Azure services and optional client IP access

  Scope:
  Resource Group
*/

@description('The name of the SQL Logical Server')
param serverName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to the database.')
param tags object = {}

@description('Your local public IP address to allow DB access (e.g., from SSMS)')
param clientIpAddress string = ''

@description('Toggle for Microsoft Defender for SQL (Advanced Threat Protection)')
param enableDefender bool = false

@description('Microsoft Entra Admin Name (e.g., your email or name)')
param entraAdminName string

@description('The Object ID of your Entra ID user/group for Admin access')
param entraAdminObjectId string

resource sqlServer 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// Entra Admin
resource entraAdmin 'Microsoft.Sql/servers/administrators@2024-05-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: entraAdminName
    sid: entraAdminObjectId
    tenantId: subscription().tenantId
  }
}

// Entra-only authentication (matches snapshot pattern)
resource aadOnly 'Microsoft.Sql/servers/azureADOnlyAuthentications@2024-05-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
}

// Firewall rules
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowClientIP 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = if (!empty(clientIpAddress)) {
  parent: sqlServer
  name: 'AllowLocalClient'
  properties: {
    startIpAddress: clientIpAddress
    endIpAddress: clientIpAddress
  }
}

resource threatProtection 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2024-05-01-preview' = if (enableDefender) {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

resource expressVA 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2024-05-01-preview' = if (enableDefender) {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}
