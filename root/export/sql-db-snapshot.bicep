param servers_sql_ipnoticehub_dev_name string = 'sql-ipnoticehub-dev'

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/sql-db-ipnoticehub-dev-weu'
  location: 'westeurope'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    owner: 'nikolay'
    region: 'weu'
    purpose: 'db'
  }
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  kind: 'v12.0,user,vcore,serverless'
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    autoPauseDelay: 60
    requestedBackupStorageRedundancy: 'Local'
    minCapacity: json('0.5')
    maintenanceConfigurationId: '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/providers/Microsoft.Maintenance/publicMaintenanceConfigurations/SQL_Default'
    isLedgerOn: false
    availabilityZone: 'NoPreference'
  }
}

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  location: 'West Europe'
  properties: {
    auditingState: 'Disabled'
  }
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/auditingSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'default'
  properties: {
    retentionDays: 0
    isAzureMonitorTargetEnabled: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource Microsoft_Sql_servers_databases_backupLongTermRetentionPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_default 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'default'
  properties: {
    weeklyRetention: 'PT0S'
    monthlyRetention: 'PT0S'
    yearlyRetention: 'PT0S'
    weekOfYear: 0
  }
}

resource Microsoft_Sql_servers_databases_backupShortTermRetentionPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_default 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'default'
  properties: {
    retentionDays: 7
    diffBackupIntervalInHours: 12
  }
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'default'
  properties: {
    retentionDays: 0
    isAzureMonitorTargetEnabled: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Current'
  properties: {}
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    state: 'Disabled'
    disabledAlerts: [
      ''
    ]
    emailAddresses: [
      ''
    ]
    emailAccountAdmins: false
    retentionDays: 0
  }
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Current'
  properties: {
    state: 'Enabled'
  }
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
    }
  }
}
