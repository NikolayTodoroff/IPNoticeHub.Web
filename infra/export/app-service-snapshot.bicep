param sites_ipnoticehub_web_lab_name string = 'ipnoticehub-web-lab'
param serverfarms_ASP_rgipnoticehublabweu_a8cc_externalid string = '/subscriptions/bcaf1056-6646-4069-8a85-c154fe786b07/resourceGroups/rg-ipnoticehub-lab-weu/providers/Microsoft.Web/serverfarms/ASP-rgipnoticehublabweu-a8cc'

resource sites_ipnoticehub_web_lab_name_resource 'Microsoft.Web/sites@2024-11-01' = {
  name: sites_ipnoticehub_web_lab_name
  location: 'West Europe'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'app-service'
  }
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${sites_ipnoticehub_web_lab_name}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${sites_ipnoticehub_web_lab_name}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: serverfarms_ASP_rgipnoticehublabweu_a8cc_externalid
    reserved: false
    isXenon: false
    hyperV: false
    dnsConfiguration: {}
    outboundVnetRouting: {
      allTraffic: false
      applicationTraffic: false
      contentShareTraffic: false
      imagePullTraffic: false
      backupRestoreTraffic: false
    }
    siteConfig: {
      numberOfWorkers: 1
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 0
      minimumElasticInstanceCount: 0
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: true
    clientAffinityProxyEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    ipMode: 'IPv4'
    customDomainVerificationId: 'D87133F40C5E2CF87F1C4C98EEB077529C441712DBD38D1A8F71C5FCBD6F9B93'
    containerSize: 0
    dailyMemoryTimeQuota: 0
    httpsOnly: true
    endToEndEncryptionEnabled: false
    redundancyMode: 'None'
    publicNetworkAccess: 'Enabled'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}

resource sites_ipnoticehub_web_lab_name_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_ipnoticehub_web_lab_name_resource
  name: 'ftp'
  location: 'West Europe'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'app-service'
  }
  properties: {
    allow: false
  }
}

resource sites_ipnoticehub_web_lab_name_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_ipnoticehub_web_lab_name_resource
  name: 'scm'
  location: 'West Europe'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'app-service'
  }
  properties: {
    allow: false
  }
}

resource sites_ipnoticehub_web_lab_name_web 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: sites_ipnoticehub_web_lab_name_resource
  name: 'web'
  location: 'West Europe'
  tags: {
    workload: 'ipnoticehub'
    env: 'lab'
    region: 'weu'
    owner: 'nikolay'
    purpose: 'app-service'
  }
  properties: {
    numberOfWorkers: 1
    defaultDocuments: [
      'Default.htm'
      'Default.html'
      'Default.asp'
      'index.htm'
      'index.html'
      'iisstart.htm'
      'default.aspx'
      'index.php'
      'hostingstart.html'
    ]
    netFrameworkVersion: 'v10.0'
    requestTracingEnabled: false
    remoteDebuggingEnabled: false
    httpLoggingEnabled: false
    acrUseManagedIdentityCreds: false
    logsDirectorySizeLimit: 35
    detailedErrorLoggingEnabled: false
    publishingUsername: 'REDACTED'
    scmType: 'None'
    use32BitWorkerProcess: true
    webSocketsEnabled: false
    alwaysOn: false
    managedPipelineMode: 'Integrated'
    virtualApplications: [
      {
        virtualPath: '/'
        physicalPath: 'site\\wwwroot'
        preloadEnabled: false
      }
    ]
    loadBalancing: 'LeastRequests'
    experiments: {
      rampUpRules: []
    }
    autoHealEnabled: false
    vnetRouteAllEnabled: false
    vnetPrivatePortsCount: 0
    publicNetworkAccess: 'Enabled'
    localMySqlEnabled: false
    managedServiceIdentityId: 12434
    ipSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictions: [
      {
        ipAddress: 'Any'
        action: 'Allow'
        priority: 2147483647
        name: 'Allow all'
        description: 'Allow all access'
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    http20Enabled: false
    minTlsVersion: '1.2'
    scmMinTlsVersion: '1.2'
    ftpsState: 'FtpsOnly'
    preWarmedInstanceCount: 0
    elasticWebAppScaleLimit: 0
    functionsRuntimeScaleMonitoringEnabled: false
    minimumElasticInstanceCount: 0
    azureStorageAccounts: {}
    http20ProxyFlag: 0
  }
}

resource sites_ipnoticehub_web_lab_name_sites_ipnoticehub_web_lab_name_azurewebsites_net 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: sites_ipnoticehub_web_lab_name_resource
  name: '${sites_ipnoticehub_web_lab_name}.azurewebsites.net'
  location: 'West Europe'
  properties: {
    siteName: 'ipnoticehub-web-lab'
    hostNameType: 'Verified'
  }
}
