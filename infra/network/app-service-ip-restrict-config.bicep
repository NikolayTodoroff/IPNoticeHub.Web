/*
  Restrict Access to App Service Advanced Tool Site (Kudu) - Configuration

  Purpose:
  - Allow personal public IP addresses to access Kudu site for App Services with Priority 100.
  - Deny all other public access to Kudu site for App Services with Priority 200.

  Scope:
  - App Service 
*/

targetScope = 'resourceGroup'

@description('App Service name.')
param appServiceName string

@description('Admin public IPv4 (single address) or CIDR (e.g. 109.120.250.37 or 109.120.250.37/32).')
param adminIpCidr string='109.120.250.37/32'

@description('If true, main site will use the same rules as SCM (ignores ipSecurityRestrictions array).')
param restrictMainSite bool = false

@description('If true, SCM will use the same rules as main site (ignores scmIpSecurityRestrictions array).')
param scmUseMainRules bool = false

resource site 'Microsoft.Web/sites@2022-09-01' existing = {
  name: appServiceName
}

// IP Restrictions Configuration
resource webConfig 'Microsoft.Web/sites/config@2022-09-01' = {
  name: 'web'
  parent: site
  properties: {
    scmIpSecurityRestrictionsUseMain: scmUseMainRules

    // Main Site
    ipSecurityRestrictions: restrictMainSite ? [
      {
        name: 'Allow-Admin-IP'
        priority: 100
        action: 'Allow'
        ipAddress: adminIpCidr
      }
      {
        name: 'Deny-All'
        priority: 200
        action: 'Deny'
        ipAddress: '0.0.0.0/0'
      }
    ] : null

    // Advanced Tool SCM / KUDU
    scmIpSecurityRestrictions: [
      {
        name: 'Allow-Admin-IP'
        priority: 100
        action: 'Allow'
        ipAddress: adminIpCidr
      }
      {
        name: 'Deny-All'
        priority: 200
        action: 'Deny'
        ipAddress: '0.0.0.0/0'
      }
      {
        name: 'Deny-All-Public-IPv6'
        priority: 300
        action: 'Deny'
        ipAddress: '::/0'
      }
    ]
    scmIpSecurityRestrictionsDefaultAction: 'Deny'
  }
}

