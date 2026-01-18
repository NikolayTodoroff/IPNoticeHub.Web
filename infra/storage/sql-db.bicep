/* PROJECT: IPNoticeHub - Secure SQL Database
  STANDARD: 2026 Cloud-Native (Express VA + Entra Only)
*/

// --- PARAMETERS ---
param serverName string
param databaseName string
param serverLocation string = resourceGroup().location
param collation string = 'SQL_Latin1_General_CP1_CI_AS'
param skuName string = 'Basic'
param tier string = 'Basic'
param maxSizeBytes int = 2147483648
param sampleName string = ''
param zoneRedundant bool = false
param licenseType string = ''
param readScaleOut string = 'Disabled'
param numberOfReplicas int = 0
param minCapacity string = ''
param autoPauseDelay int = 0
param databaseTags object = {}
param enableADS bool = true
param enableVA bool = true
param availabilityZone string = 'NoPreference'
param useFreeLimit bool = false
param freeLimitExhaustionBehavior string = ''

@description('Admin email for Entra ID login')
param entraAdminName string

@description('Object ID of the Entra ID Admin')
param entraAdminObjectId string

// --- LOGICAL SERVER ---
resource server 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: serverLocation
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
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
resource database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: server
  name: databaseName
  location: serverLocation
  tags: databaseTags
  sku: {
    name: skuName
    tier: tier
  }
  properties: {
    collation: collation
    maxSizeBytes: maxSizeBytes
    sampleName: sampleName
    zoneRedundant: zoneRedundant
    licenseType: licenseType
    readScale: readScaleOut
    highAvailabilityReplicaCount: numberOfReplicas
    minCapacity: minCapacity
    autoPauseDelay: autoPauseDelay
    isLedgerOn: false
    availabilityZone: availabilityZone
  }
}

// --- SECURITY: MODERN EXPRESS CONFIGURATION (No Storage Needed) ---

// 1. Advanced Threat Protection (ADS)
resource threatProtection 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2023-05-01-preview' = if (enableADS) {
  parent: server
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

// 2. Express Vulnerability Assessment (VA)
resource expressVA 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2023-05-01-preview' = if (enableVA) {
  parent: server
  name: 'default'
  properties: {
    state: 'Enabled'
  }
}

/* --- DEPRECATED: CLASSIC STORAGE-BASED VA ---
  The resources below are commented out as they are no longer recommended for 2026 deployments.
  Using 'Express VA' (above) avoids the $ / complexity of a Storage Account.

  resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = if (false) { ... }
  resource classicVA 'Microsoft.Sql/servers/vulnerabilityAssessments@2018-06-01-preview' = if (false) { ... }
*/

output sqlServerFqdn string = server.properties.fullyQualifiedDomainName
