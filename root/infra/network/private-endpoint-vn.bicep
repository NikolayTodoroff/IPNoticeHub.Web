/*
  Private Endpoint for a resource with Private DNS Zones

  Purpose:
  - Creates a Private Endpoint for a resource
  - Associates the Private Endpoint with a Private DNS Zone

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param location string
param tags object = {}

@description('Subnet resource ID where the private endpoint NIC will be created.')
param subnetId string

@description('Name of the Private Endpoint resource.')
param privateEndpointName string

@description('Resource ID of the target Private Link resource (e.g., SQL server, Key Vault, Storage account).')
param privateLinkResourceId string

@description('Private Link group IDs ([\'sqlServer\'] for SQL, [\'vault\'] for Key Vault, [\'blob\'] for Storage Blob).')
param groupIds array

@description('Resource ID of the Private DNS Zone to link (e.g., privatelink.database.windows.net zone id).')
param privateDnsZoneId string

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2024-07-01' = {
  name: privateEndpointName
  location: location
  tags: union(tags, { purpose: 'network' })
  properties: {
    subnet: {
      id: subnetId
    }

    customNetworkInterfaceName: '${privateEndpointName}-nic'
    
    privateLinkServiceConnections: [
      {
        name: '${privateEndpointName}-pls'
        properties: {
          privateLinkServiceId: privateLinkResourceId
          groupIds: groupIds
        }
      }
    ]
  }
}

resource dnsZoneGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2024-07-01' = {
  name: 'default'
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'dnsZoneConfig'
        properties: {
          privateDnsZoneId: privateDnsZoneId
        }
      }
    ]
  }
}

output privateEndpointId string = privateEndpoint.id
output privateEndpointNicId string = privateEndpoint.properties.networkInterfaces[0].id



