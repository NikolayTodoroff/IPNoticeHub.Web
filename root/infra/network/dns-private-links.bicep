/*
  Private DNS Zones and VNet Links

  Purpose:
  - Creates Private DNS Zones for database, Key Vault, and Blob Storage
  - Links the existing Virtual Network to these Private DNS Zones

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param vnetName string
param tags object = {}

param dbPrivateDnsName string
param kvPrivateDnsName string
param blobPrivateDnsName string

// Reference existing VNet
resource vnet 'Microsoft.Network/virtualNetworks@2025-05-01' existing = {
  name: vnetName
}

// Create Private DNS Zones for DB, Key Vault, and Blob Storage
resource dbPrivateDns 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: dbPrivateDnsName
  location: 'global'
  tags: union(tags, { purpose: 'network' })
}

resource kvPrivateDns 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: kvPrivateDnsName
  location: 'global'
  tags: union(tags, { purpose: 'network' })
}

resource blobPrivateDns 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  name: blobPrivateDnsName
  location: 'global'
  tags: union(tags, { purpose: 'network' })
}

// Link VNet to Private DNS Zones
resource vnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: dbPrivateDns
  name: 'link-${vnetName}-db'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

resource vnetLinkKv 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: kvPrivateDns
  name: 'link-${vnetName}-kv'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

resource vnetLinkBlob 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: blobPrivateDns
  name: 'link-${vnetName}-blob'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnet.id
    }
  }
}

output privateDnsZoneIds object = {
  sql: dbPrivateDns.id
  keyVault: kvPrivateDns.id
  blob: blobPrivateDns.id
}
