/*
  Short and Long Term Backup Retention Configuration for Azure SQL Databases

  Purpose:
  - Enforce short-term and long-term backup retention configuration for Azure SQL Databases to ensure data protection and compliance with organizational requirements.

  Scope:
  - SQL Databases within the specified resource group.
*/

targetScope = 'resourceGroup'

param sqlServerName string
param sqlDatabaseName string

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' existing = {
  name: '${sqlServerName}/${sqlDatabaseName}'
}

resource shortTermBackup 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2024-11-01-preview' = {
  parent: sqlDatabase
  name: 'default'
  properties: {
    diffBackupIntervalInHours: 12
    retentionDays: 14
  }
}

resource longTermBackup 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2024-11-01-preview' = {
  parent: sqlDatabase
  name: 'default'
  properties: {
    weekOfYear: 16
    weeklyRetention: 'P8W'
    monthlyRetention: 'P6M'
    yearlyRetention: 'P1Y'
  }
}
