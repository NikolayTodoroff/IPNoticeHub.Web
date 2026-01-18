/* SCOPE: Resource Group
   PROJECT: IPNoticeHub Lab
   DESCRIPTION: Secure SQL Server & DB using Entra ID, Express VA, and ATP.
   AUTHORS: Nikolay & AI Thought Partner
*/

// --- PARAMETERS ---
@description('The name of the SQL Logical Server')
param serverName string

@description('The name of the SQL Database')
param sqlDbName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Your local public IP address to allow DB access (e.g., from SSMS)')
param clientIpAddress string = ''

@description('Toggle for Microsoft Defender for SQL (Advanced Threat Protection)')
param enableDefender bool = true

@description('Microsoft Entra Admin Name (e.g., your email or name)')
param entraAdminName string

@description('The Object ID of your Entra ID user/group for Admin access')
param entraAdminObjectId string

// --- LOGICAL SQL SERVER ---
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    // ENTRA-ONLY AUTH: No SQL passwords allowed
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: entraAdminName
      sid: entraAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true 
    }
  }
}

// --- SQL DATABASE ---
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
  sku: {
    name: 'Basic' // Budget-friendly for lab/learning
    tier: 'Basic'
    capacity: 5
  }
}

// --- FIREWALL RULES ---

// Rule 1: Allow Azure Services (Required for Container Apps)
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Rule 2: Allow Your Local Machine (Only if IP is provided)
resource allowClientIP 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = if (!empty(clientIpAddress)) {
  parent: sqlServer
  name: 'AllowLocalClient'
  properties: {
    startIpAddress: clientIpAddress
    endIpAddress: clientIpAddress
  }
}

// --- SECURITY: DEFENDER & EXPRESS VA ---

// Microsoft Defender for SQL (Advanced Threat Protection)
resource threatProtection 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2023-05-01-preview' = if (enableDefender) {
  parent: sqlServer
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

// Express Vulnerability Assessment (No Storage Account Needed)
resource expressVA 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2023-05-01-preview' = if (enableDefender) {
  parent: sqlServer
  name: 'default'
  properties: {
    state: 'Enabled'
  }
}
