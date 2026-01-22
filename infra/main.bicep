@description('Environment name like lab/dev/prod')
param env string = 'lab'

@description('Owner tag')
param owner string = 'nikolay'

@description('Region tag (your custom tag value, not Azure location)')
param regionTag string = 'weu'

@description('Workload/app name')
param workload string = 'ipnoticehub'

// Common tags used across all deployments
var commonTags = {
  env: env
  owner: owner
  region: regionTag
  workload: workload
}

// SQL Server module
module sqlServer 'storage/sql-server.bicep' = {
  name: 'sqlServer'
  params: {
    serverName: 'ipnoticehub-sql' 
    location: resourceGroup().location
    tags: union(commonTags, { purpose: 'db' })
    entraAdminName: 'admin@yourdomain.com'
    entraAdminObjectId: 'your-entra-admin-object-id'
  }
}

// SQL DB module
module sqlDb 'storage/sql-db.bicep' = {
  name: 'sqlDb'
  params: {
    serverName: 'ipnoticehub-sql'
    databaseName: 'ipnoticehub-db'
    location: resourceGroup().location
    tags: union(commonTags, { purpose: 'db' })
  }
}
