/*
  Entra ID Security Groups

  Purpose:
  - Maintains references to existing security groups within the specified Entra ID (Azure AD) tenant.

  Scope:
  - Entra ID Tenant
*/

targetScope = 'tenant'
extension microsoftGraphV1

param adminSecurityGroupName string
param userSecurityGroupName string

resource adminGroup 'Microsoft.Graph/groups@v1.0' existing = {
  uniqueName: adminSecurityGroupName
}

resource userGroup 'Microsoft.Graph/groups@v1.0' existing = {
  uniqueName: userSecurityGroupName
}

output adminGroupId string = adminGroup.id
output userGroupId string = userGroup.id

