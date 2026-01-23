/*
  Entra ID Security Groups

  Purpose:
  - Maintains references to existing security groups within the specified Entra ID (Azure AD) tenant.

  Scope:
  - Entra ID Tenant
*/

targetScope = 'tenant'
extension microsoftGraphV1

resource adminGroup 'Microsoft.Graph/groups@v1.0' existing = {
  uniqueName: 'IPNoticeHub-App-Admins'
}

resource userGroup 'Microsoft.Graph/groups@v1.0' existing = {
  uniqueName: 'IPNoticeHub-App-Users'
}

output adminGroupId string = adminGroup.id
output userGroupId string = userGroup.id

