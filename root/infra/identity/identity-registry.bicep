/*
  Identity Registry

  Purpose:
  - Maintains a registry of identities within the specified Entra ID (Azure AD) tenant.

  Scope:
  - Entra ID Tenant
*/

targetScope = 'tenant'
extension microsoftGraphV1

param breakGlassUpn string
param globalAdminUpn string
param sqlAdminUpn string
param testUserUpn string

resource breakGlassAdmin 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: breakGlassUpn
}

resource globalAdmin 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: globalAdminUpn
}

resource sqlAdmin 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: sqlAdminUpn
}

resource testUser 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: testUserUpn
}

output breakGlassObjectId string = breakGlassAdmin.id
output breakGlassUpn string = breakGlassAdmin.userPrincipalName

output globalAdminObjectId string = globalAdmin.id
output globalAdminUpn string = globalAdmin.userPrincipalName

output sqlAdminObjectId string = sqlAdmin.id
output sqlAdminUpn string = sqlAdmin.userPrincipalName

output testUserObjectId string = testUser.id
output testUserUpn string = testUser.userPrincipalName
