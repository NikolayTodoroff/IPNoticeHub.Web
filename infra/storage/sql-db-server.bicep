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

param serverName string
param location string = resourceGroup().location
param tags object = {}
param clientIpAddress string = ''
param enableDefender bool = false

resource sqlServer 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: serverName
  location: location
  tags: union(tags, { purpose: 'storage' })
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
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

output serverName string = sqlServer.name
output serverId string = sqlServer.id
