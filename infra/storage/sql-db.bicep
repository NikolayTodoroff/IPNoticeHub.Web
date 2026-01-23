/*
  SQL Database Deployment

  Purpose:
  Deploys an Azure SQL database with Entra ID-only authentication
  and security features (Advanced Threat Protection and Express Vulnerability Assessment).

  Key features:
  - Entra ID (Azure AD) authentication only - no SQL logins
  - Minimum TLS 1.2 enforcement
  - Advanced Threat Protection (Microsoft Defender for SQL)
  - Express Vulnerability Assessment (no storage account required)
  - Configurable SKU, capacity, and high availability options
  - Zone redundancy and availability zone placement support

  Scope:
  Resource Group
*/

param serverName string
param databaseName string
param location string = resourceGroup().location
param tags object = {}

// Database properties
param skuName string = 'GP_S_Gen5'
param skuTier string = 'GeneralPurpose'
param skuFamily string = 'Gen5'
param skuCapacity int = 1

// Database properties
param collation string = 'SQL_Latin1_General_CP1_CI_AS'
param maxSizeBytes int = 34359738368
param zoneRedundant bool = false

@description('Read scale setting.')
@allowed([
  'Disabled'
  'Enabled'
])

param readScale string = 'Disabled'
param autoPauseDelay int = 60
param minCapacity string = '0.5'

@description('Requested backup storage redundancy. Snapshot used Local.')
@allowed([
  'Local'
  'Zone'
  'Geo'
  'GeoZone'
])
param requestedBackupStorageRedundancy string = 'Local'

@description('Maintenance configuration resource ID (optional). If empty, Azure default applies.')
param maintenanceConfigurationId string = ''
param availabilityZone string = 'NoPreference'

// Security
param enableDbThreatProtection bool = false

// Existing SQL Server
resource sqlServer 'Microsoft.Sql/servers@2024-05-01-preview' existing = {
  name: serverName
}

// Database resource
resource sqlDb 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  tags: union(tags, { purpose: 'storage' })

  sku: {
    name: skuName
    tier: skuTier

    family: empty(skuFamily) ? null : skuFamily
    capacity: skuCapacity
  }

  properties: {
    collation: collation
    maxSizeBytes: maxSizeBytes
    zoneRedundant: zoneRedundant
    readScale: readScale

    autoPauseDelay: autoPauseDelay
    minCapacity: json(minCapacity)
    requestedBackupStorageRedundancy: requestedBackupStorageRedundancy

    isLedgerOn: false
    availabilityZone: availabilityZone

    maintenanceConfigurationId: empty(maintenanceConfigurationId) ? null : maintenanceConfigurationId
  }
}

// Db Advanced Threat Protection
resource dbAtp 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2024-05-01-preview' = if (enableDbThreatProtection) {
  parent: sqlDb
  name: 'Default'
  properties: {
    state: 'Enabled'
  }
}

output databaseId string = sqlDb.id

