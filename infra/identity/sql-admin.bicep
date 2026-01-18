/* SQL ADMIN CONFIGURATION (2026 Standard)
    - Sets up an Entra ID admin for the SQL Server
    - Enforces Entra ID-only authentication
*/

param serverName string = 'sql-ipnoticehub-lab'
param location string = resourceGroup().location

// 1. The Entra ID Admin Details
@description('The display name or UPN of the Entra admin (e.g., your email)')
param entraAdminName string = 'nikolay.todorov@ipnoticehub.com'

@description('The Object ID from the Entra ID portal for your user')
param entraAdminObjectId string // Paste your GUID here

// --- SQL SERVER ---
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: serverName
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    // This blocks all SQL passwords globally on this server
    publicNetworkAccess: 'Enabled' 
  }
}

// --- ENTRA ADMIN ASSIGNMENT ---
resource sqlAdmin 'Microsoft.Sql/servers/administrators@2023-05-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory' // This must be exactly 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: entraAdminName
    sid: entraAdminObjectId
    tenantId: subscription().tenantId
    principalType: 'User' //
  }
}

// --- ENFORCE ENTRA-ONLY AUTH ---
resource entraOnly 'Microsoft.Sql/servers/azureADOnlyAuthentications@2023-05-01-preview' = {
  parent: sqlServer
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true // Disables SQL login entirely
  }
}
