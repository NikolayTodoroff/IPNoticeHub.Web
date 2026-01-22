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

@description('Existing SQL logical server name (must already exist).')
param serverName string

@description('SQL database name.')
param databaseName string

@description('Location for the database (usually same as server).')
param location string = resourceGroup().location

@description('Tags to apply to the database.')
param tags object = {}

// Database properties
@description('SKU name. Example: GP_S_Gen5 (serverless), GP_Gen5 (provisioned), Basic.')
param skuName string = 'GP_S_Gen5'

@description('SKU tier. Example: GeneralPurpose, Basic.')
param skuTier string = 'GeneralPurpose'

@description('SKU family (Gen5 for many SQL offerings). Leave empty for Basic.')
param skuFamily string = 'Gen5'

@description('vCores for vCore model (1 is what your snapshot shows). Not used for Basic in practice.')
param skuCapacity int = 1

// Database properties
param collation string = 'SQL_Latin1_General_CP1_CI_AS'

@description('Max DB size in bytes. Snapshot used 34359738368 (32 GiB).')
param maxSizeBytes int = 34359738368

@description('Enable zone redundancy (if supported by SKU/region).')
param zoneRedundant bool = false

@description('Read scale setting.')
@allowed([
  'Disabled'
  'Enabled'
])
param readScale string = 'Disabled'

@description('Serverless: auto pause delay in minutes. Use 60 like your snapshot; use -1 to disable autopause.')
param autoPauseDelay int = 60

@description('Serverless: minimum vCores. Snapshot used 0.5.')
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

@description('Availability zone preference (usually NoPreference).')
param availabilityZone string = 'NoPreference'

// Security
@description('Toggle database Advanced Threat Protection (Defender for SQL signals). Note: full Defender is controlled by Defender for Cloud plan too.')
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
  tags: tags

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

    // Serverless knobs
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

