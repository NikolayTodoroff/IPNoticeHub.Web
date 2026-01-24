/*
  Storage Account 

  Purpose:
  - A comprehensive storage definition including Blob and File services. 
  - Implements Soft-Delete protection and specific Security/Networking ACLs.

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param storageAccountName string = 'stipnoticehubdocslabweu'
param location string = resourceGroup().location
param tags object = {}

@allowed([
  'Standard_LRS'
  'Standard_ZRS'
  'Standard_GRS'
  'Standard_RAGRS'
])
param skuName string = 'Standard_LRS'

@allowed([
  'StorageV2'
])
param kind string = 'StorageV2'

@allowed([
  'Hot'
  'Cool'
])
param accessTier string = 'Cool'

// Security defaults
param minimumTlsVersion string = 'TLS1_2'
param allowBlobPublicAccess bool = false
param allowSharedKeyAccess bool = true
param defaultToOAuthAuthentication bool = true

// Networking defaults (lab-friendly; later you can lock down)
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

@allowed([
  'Allow'
  'Deny'
])
param networkDefaultAction string = 'Allow'

@allowed([
  'None'
  'AzureServices'
])
param networkBypass string = 'AzureServices'

param ipRules array = []

// Soft delete defaults
param blobSoftDeleteDays int = 7
param containerSoftDeleteDays int = 7
param shareSoftDeleteDays int = 7

// Containers
param containerNames array = [
  'legal-notices'
  'exports'
  'app-assets'
]

resource storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  tags: union(tags, { purpose: 'storage' })
  sku: {
    name: skuName
  }
  kind: kind
  properties: {
    accessTier: accessTier
    minimumTlsVersion: minimumTlsVersion
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: allowBlobPublicAccess
    allowSharedKeyAccess: allowSharedKeyAccess
    defaultToOAuthAuthentication: defaultToOAuthAuthentication
    publicNetworkAccess: publicNetworkAccess
    allowCrossTenantReplication: false

    networkAcls: {
      bypass: networkBypass
      defaultAction: networkDefaultAction
      ipRules: ipRules
    }

    encryption: {
      keySource: 'Microsoft.Storage'
      services: {
        blob: { enabled: true }
        file: { enabled: true }
        queue: { enabled: true }
        table: { enabled: true }
      }
      requireInfrastructureEncryption: false
    }
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: blobSoftDeleteDays
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: containerSoftDeleteDays
    }
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2024-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    shareDeleteRetentionPolicy: {
      enabled: true
      days: shareSoftDeleteDays
    }
  }
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [for c in containerNames: {
  parent: blobService
  name: c
}]

output storageAccountId string = storage.id
output storageAccountNameOut string = storage.name
output blobEndpoint string = storage.properties.primaryEndpoints.blob
