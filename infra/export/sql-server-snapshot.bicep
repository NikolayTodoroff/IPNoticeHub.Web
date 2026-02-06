@secure()
param vulnerabilityAssessments_Default_storageContainerPath string
param servers_sql_ipnoticehub_dev_name string = 'sql-ipnoticehub-dev'

resource servers_sql_ipnoticehub_dev_name_resource 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: servers_sql_ipnoticehub_dev_name
  location: 'westeurope'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    owner: 'nikolay'
    region: 'weu'
    purpose: 'db'
  }
  kind: 'v12.0'
  properties: {
    administratorLogin: 'CloudSAb70e309b'
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: 'nikolay.todorov@ipnoticehub.com'
      sid: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
      tenantId: '227c0e16-85bf-4c81-a620-0ce328414830'
      azureADOnlyAuthentication: true
    }
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_ActiveDirectory 'Microsoft.Sql/servers/administrators@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: 'nikolay.todorov@ipnoticehub.com'
    sid: '805ed398-1b8b-4b1d-bfa7-bdd99bea4e4a'
    tenantId: '227c0e16-85bf-4c81-a620-0ce328414830'
  }
}

resource servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_CreateIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_DbParameterization 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_DefragmentIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_DropIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_sql_ipnoticehub_dev_name_ForceLastGoodPlan 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
}

resource Microsoft_Sql_servers_auditingPolicies_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/auditingPolicies@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  location: 'West Europe'
  properties: {
    auditingState: 'Disabled'
  }
}

resource Microsoft_Sql_servers_auditingSettings_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/auditingSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource Microsoft_Sql_servers_azureADOnlyAuthentications_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/azureADOnlyAuthentications@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
}

resource Microsoft_Sql_servers_connectionPolicies_servers_sql_ipnoticehub_dev_name_default 'Microsoft.Sql/servers/connectionPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'default'
  location: 'westeurope'
  properties: {
    connectionType: 'Default'
  }
}

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'sql-db-ipnoticehub-dev-weu'
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

resource servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  location: 'West Europe'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/auditingSettings@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  properties: {
    retentionDays: 0
    isAzureMonitorTargetEnabled: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  properties: {
    retentionDays: 0
    isAzureMonitorTargetEnabled: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource servers_sql_ipnoticehub_dev_name_master_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Current'
  properties: {}
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_sql_ipnoticehub_dev_name_master_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Current'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_sql_ipnoticehub_dev_name_master_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2024-05-01-preview' = {
  name: '${servers_sql_ipnoticehub_dev_name}/master/Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
    }
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_devOpsAuditingSettings_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/devOpsAuditingSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  properties: {
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_sql_ipnoticehub_dev_name_current 'Microsoft.Sql/servers/encryptionProtector@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'current'
  kind: 'servicemanaged'
  properties: {
    serverKeyName: 'ServiceManaged'
    serverKeyType: 'ServiceManaged'
    autoRotationEnabled: false
  }
}

resource Microsoft_Sql_servers_extendedAuditingSettings_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/extendedAuditingSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'default'
  properties: {
    retentionDays: 0
    auditActionsAndGroups: []
    isStorageSecondaryKeyInUse: false
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_sql_ipnoticehub_dev_name_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource servers_sql_ipnoticehub_dev_name_dev_home_ip 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'dev-home-ip'
  properties: {
    startIpAddress: '109.120.250.37'
    endIpAddress: '109.120.250.37'
  }
}

resource servers_sql_ipnoticehub_dev_name_ServiceManaged 'Microsoft.Sql/servers/keys@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'ServiceManaged'
  kind: 'servicemanaged'
  properties: {
    serverKeyType: 'ServiceManaged'
  }
}

resource Microsoft_Sql_servers_securityAlertPolicies_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/securityAlertPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
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

resource Microsoft_Sql_servers_sqlVulnerabilityAssessments_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource Microsoft_Sql_servers_vulnerabilityAssessments_servers_sql_ipnoticehub_dev_name_Default 'Microsoft.Sql/servers/vulnerabilityAssessments@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_resource
  name: 'Default'
  properties: {
    recurringScans: {
      isEnabled: false
      emailSubscriptionAdmins: true
    }
    storageContainerPath: vulnerabilityAssessments_Default_storageContainerPath
  }
}

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  location: 'West Europe'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupShortTermRetentionPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_default 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'default'
  properties: {
    retentionDays: 7
    diffBackupIntervalInHours: 12
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Current'
  properties: {}
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2024-05-01-preview' = {
  parent: servers_sql_ipnoticehub_dev_name_sql_db_ipnoticehub_dev_weu
  name: 'Current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
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
  dependsOn: [
    servers_sql_ipnoticehub_dev_name_resource
  ]
}
