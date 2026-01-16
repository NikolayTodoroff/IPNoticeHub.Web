/* RESOURCE MODULE: Advanced Azure Storage Account
   PROJECT: IPNoticeHub
   DATE: 2026-01-15
   
   DESCRIPTION: 
   A comprehensive storage definition including Blob and File services. 
   Implements Soft-Delete protection and specific Security/Networking ACLs.
   
   GOVERNANCE: 
   Supports tagging and mandatory TLS 1.2+ for compliance.
*/

// --- PARAMETERS ---

@description('Target region for the storage account.')
param location string = resourceGroup().location

@description('Global unique name: stipnoticehubdocslabweu')
param storageAccountName string = 'stipnoticehubdocslabweu'

@description('SKU Tier (e.g., Standard_LRS, Standard_GRS)')
param accountType string = 'Standard_LRS'

@description('Storage Kind (v2 is standard for modern apps)')
param kind string = 'StorageV2'

@description('Access Tier (Hot for active data, Cool for backups)')
param accessTier string = 'Hot'

// Security Parameters
param minimumTlsVersion string = 'TLS1_2'
param supportsHttpsTrafficOnly bool = true
param allowBlobPublicAccess bool = false
param allowSharedKeyAccess bool = true
param defaultOAuth bool = true

// Networking Parameters
param publicNetworkAccess string = 'Enabled'
param networkAclsBypass string = 'AzureServices'
param networkAclsDefaultAction string = 'Allow'
param networkAclsIpRules array = []
param networkAclsIpv6Rules array = []

// Soft Delete / Protection Parameters
param isBlobSoftDeleteEnabled bool = true
param blobSoftDeleteRetentionDays int = 7
param isContainerSoftDeleteEnabled bool = true
param containerSoftDeleteRetentionDays int = 7
param isShareSoftDeleteEnabled bool = true
param shareSoftDeleteRetentionDays int = 7

// Encryption / Advanced Parameters
param encryptionEnabled bool = true
param infrastructureEncryptionEnabled bool = false
param keySource string = 'Microsoft.Storage'
param dnsEndpointType string = 'Standard'
param publishIpv6Endpoint bool = false
param largeFileSharesState string = 'Disabled'
param allowCrossTenantReplication bool = false

// --- RESOURCES ---

@description('Main Storage Account Resource')
resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: storageAccountName
  location: location
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'storage'
  }
  sku: {
    name: accountType
  }
  kind: kind
  properties: {
    minimumTlsVersion: minimumTlsVersion
    supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
    allowBlobPublicAccess: allowBlobPublicAccess
    allowSharedKeyAccess: allowSharedKeyAccess
    defaultToOAuthAuthentication: defaultOAuth
    accessTier: accessTier
    publicNetworkAccess: publicNetworkAccess
    allowCrossTenantReplication: allowCrossTenantReplication
    networkAcls: {
      bypass: networkAclsBypass
      defaultAction: networkAclsDefaultAction
      ipRules: networkAclsIpRules
      ipv6Rules: networkAclsIpv6Rules
    }
    dualStackEndpointPreference: {
      publishIpv6Endpoint: publishIpv6Endpoint
    }
    dnsEndpointType: dnsEndpointType
    largeFileSharesState: largeFileSharesState
    encryption: {
      keySource: keySource
      services: {
        blob: { enabled: encryptionEnabled }
        file: { enabled: encryptionEnabled }
        table: { enabled: encryptionEnabled }
        queue: { enabled: encryptionEnabled }
      }
      requireInfrastructureEncryption: infrastructureEncryptionEnabled
    }
  }
}

@description('Blob Service configuration including data protection (Soft Delete)')
resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: isBlobSoftDeleteEnabled
      days: blobSoftDeleteRetentionDays
    }
    containerDeleteRetentionPolicy: {
      enabled: isContainerSoftDeleteEnabled
      days: containerSoftDeleteRetentionDays
    }
  }
}

@description('File Service configuration for SMB/File shares')
resource fileServices 'Microsoft.Storage/storageAccounts/fileServices@2025-06-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    shareDeleteRetentionPolicy: {
      enabled: isShareSoftDeleteEnabled
      days: shareSoftDeleteRetentionDays
    }
  }
}

var containerNames = [
  'legal-notices'
  'exports'
  'app-assets'
]

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [for name in containerNames: {
  // Name pattern: <AccountName>/default/<ContainerName>
  name: '${storageAccount.name}/default/${name}'
}]
