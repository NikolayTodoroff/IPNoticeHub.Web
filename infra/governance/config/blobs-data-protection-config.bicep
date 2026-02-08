/*
  Blobs Data Protection Configuration

  Purpose:
  - Enables and configures versioning for blob storage accounts to ensure data protection and recovery capabilities.
  - Enables blobs change feed to track changes to blob storage accounts for auditing and monitoring purposes.
  - Configures soft delete for blobs and containers to allow recovery of accidentally deleted data.
  - Optionally enables last access time tracking for lifecycle management scenarios.
  - Optionally enables Point-in-Time Restore (PITR) for blob storage accounts to allow recovery to a specific point in time.

  Scope:
  - Blobs storage accounts
*/

targetScope = 'resourceGroup'

@description('Name of the existing Storage Account.')
param storageAccountName string

@description('Enable blob versioning (recommended for recovery).')
param enableVersioning bool = true

@description('Enable blob change feed.')
param enableChangeFeed bool = true

@description('Optional: automatically retain change feed data for N days. Set to 0 to omit retention.')
@minValue(0)
@maxValue(146000)
param changeFeedRetentionInDays int = 7

@description('Enable soft delete for blobs.')
param enableBlobSoftDelete bool = true

@description('Blob soft delete retention (days).')
@minValue(1)
@maxValue(365)
param blobSoftDeleteDays int = 7

@description('Enable soft delete for containers.')
param enableContainerSoftDelete bool = true

@description('Container soft delete retention (days).')
@minValue(1)
@maxValue(365)
param containerSoftDeleteDays int = 7

@description('Allow permanent delete of soft-deleted blobs/containers. Keep false unless you have a clear governance reason.')
param allowPermanentDelete bool = false

@description('Enable last access time tracking (needed if lifecycle rules use last-access-time conditions).')
param enableLastAccessTimeTracking bool = false

@description('Tracking granularity in days (commonly 1). Only relevant if last access tracking enabled.')
@minValue(1)
@maxValue(365)
param trackingGranularityInDays int = 1

@description('Enable Point-in-Time Restore (PITR). Costs/requirements apply; typically left off in labs unless specifically testing.')
param enablePointInTimeRestore bool = false

@description('PITR retention in days. Only used when PITR enabled.')
@minValue(1)
@maxValue(365)
param pointInTimeRestoreDays int = 7

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {

    isVersioningEnabled: enableVersioning

    changeFeed: union(
      {
        enabled: enableChangeFeed
      },
      changeFeedRetentionInDays > 0
        ? { retentionInDays: changeFeedRetentionInDays }
        : {}
    )

    deleteRetentionPolicy: {
      enabled: enableBlobSoftDelete
      days: blobSoftDeleteDays
      allowPermanentDelete: allowPermanentDelete
    }

    containerDeleteRetentionPolicy: {
      enabled: enableContainerSoftDelete
      days: containerSoftDeleteDays
      allowPermanentDelete: allowPermanentDelete
    }

    lastAccessTimeTrackingPolicy: enableLastAccessTimeTracking ? {
      enable: true
      trackingGranularityInDays: trackingGranularityInDays
      blobType: ['blockBlob']
    } : null

    restorePolicy: enablePointInTimeRestore ? {
      enabled: true
      days: pointInTimeRestoreDays
    } : null
  }
}

