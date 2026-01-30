var vnetName = 'vn-${workload}-${env}-${region}'

param env string 
param owner string
param region string
param workload string

param location string
var globalTags = {
  env: env
  owner: owner
  region: region
  workload: workload
}

param appSvcSubnetName string
param peSubnetName string
param mgmtSubnetName string
param dbPrivateDnsName string
param kvPrivateDnsName string
param blobPrivateDnsName string
param sqlServerResourceId string
param keyVaultResourceId string
param storageAccountResourceId string

module vNet 'vnet-subnets.bicep' = {
  name: 'vNet'
  params: {
    vnetName: vnetName
    location: location
    tags: globalTags
    appSvcSubnetName: appSvcSubnetName
    peSubnetName: peSubnetName
    mgmtSubnetName: mgmtSubnetName
  }
}

module privateDnsLinks 'dns-private-links.bicep' = {
  name: 'privateDnsLinks'
  params: {
    vnetName: vnetName
    tags: globalTags
    dbPrivateDnsName: dbPrivateDnsName
    kvPrivateDnsName: kvPrivateDnsName
    blobPrivateDnsName: blobPrivateDnsName
  }
}

module dbPrivateEndpoint 'private-endpoint-vn.bicep' = {
  name: 'dbPrivateEndpoint'
  params: {
    location: location
    tags: globalTags
    subnetId: vNet.outputs.peSubnetId
    privateEndpointName: 'pe-db-${workload}-${env}-${region}'

    privateDnsZoneId: privateDnsLinks.outputs.privateDnsZoneIds.dbPrivateDnsZoneId
    privateLinkResourceId: sqlServerResourceId
    groupIds:['sqlServer']
  }
}

module kvPrivateEndpoint 'private-endpoint-vn.bicep' = {
  name: 'kvPrivateEndpoint'
  params: {
    location: location
    tags: globalTags
    subnetId: vNet.outputs.peSubnetId
    privateEndpointName: 'pe-kv-${workload}-${env}-${region}'

    privateDnsZoneId: privateDnsLinks.outputs.privateDnsZoneIds.kvPrivateDnsZoneId
    privateLinkResourceId: keyVaultResourceId
    groupIds:['vault']
  }
}

module blobPrivateEndpoint 'private-endpoint-vn.bicep' = {
  name: 'blobPrivateEndpoint'
  params: {
    location: location
    tags: globalTags
    subnetId: vNet.outputs.peSubnetId
    privateEndpointName: 'pe-blob-${workload}-${env}-${region}'

    privateDnsZoneId: privateDnsLinks.outputs.privateDnsZoneIds.blobPrivateDnsZoneId
    privateLinkResourceId: storageAccountResourceId
    groupIds:['blob']
  }
}
