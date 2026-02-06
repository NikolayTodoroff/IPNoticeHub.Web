/*
  Virtual Network and Subnets

  Purpose:
  - Creates a Virtual Network with three subnets:
    - App Service subnet with delegation to Microsoft.Web/serverFarms
    - Private Endpoint subnet with private endpoint network policies disabled
    - Management subnet

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

param location string
param tags object = {}
param vnetName string

param appSvcSubnetName string
param peSubnetName string
param mgmtSubnetName string

@description('VNet address prefixes')
param vnetAddressPrefixes array = [
  '10.0.0.0/16'
]

@description('Subnet configurations')
param subnetConfigs array = [
  {
    name: appSvcSubnetName
    addressPrefix: '10.0.0.0/26'
    delegationServiceName: 'Microsoft.Web/serverFarms'
  }
  {
    name: peSubnetName
    addressPrefix: '10.0.1.0/26'
    privateEndpointNetworkPolicies: 'Disabled'
  }
  {
    name: mgmtSubnetName
    addressPrefix: '10.0.2.0/26'
  }
]

resource vnet 'Microsoft.Network/virtualNetworks@2024-07-01' = {
  name: vnetName
  location: location
  tags: union(tags, { purpose: 'network' })
  properties: {
    addressSpace: {
      addressPrefixes: vnetAddressPrefixes
    }
    subnets: [
      for s in subnetConfigs: {
        name: s.name
        properties: {
          addressPrefix: s.addressPrefix

          // Private Endpoint subnet specific settings
          privateEndpointNetworkPolicies: s.?privateEndpointNetworkPolicies ?? 'Enabled'

          // Private Link Service subnet specific settings
          privateLinkServiceNetworkPolicies: s.?privateLinkServiceNetworkPolicies ?? 'Enabled'

          // Delegation for App Service subnet
          delegations: s.?delegationServiceName != null
            ? [
                {
                  name: 'delegation'
                  properties: {
                    serviceName: s.delegationServiceName
                  }
                }
              ]
            : null
        }
      }
    ]
  }
}

output vnetId string = vnet.id
output appSvcSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, appSvcSubnetName)
output peSubnetId string  = resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, peSubnetName)
output mgmtSubnetId string = resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, mgmtSubnetName)


