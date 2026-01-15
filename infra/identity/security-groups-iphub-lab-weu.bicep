/* RESOURCE MODULE: Entra ID Security Groups & Memberships
   PROJECT: IPNoticeHub
   DATE: 2026-01-14
   
   DESCRIPTION: 
   This template defines the security groups for the IPNoticeHub application.
   It also automates the assignment of specific users to these groups.
   
   SECURITY NOTE: 
   The 'Emergency Admin' is intentionally excluded from all groups to maintain 
   isolation as a 'Break-Glass' account.
*/

extension microsoftGraph

// --- EXISTING USER REFERENCES ---

resource defaultAdminUser 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: 'everflowing555@gmail.com'
}

resource standardTestUser 'Microsoft.Graph/users@v1.0' existing = {
  userPrincipalName: 'Test-User@ipnoticehub.com'
}

// --- SECURITY GROUPS ---

@description('Group for administrative users with full control over the application.')
resource adminGroup 'Microsoft.Graph/groups@v1.0' = {
  displayName: 'IPNoticeHub-App-Admins'
  description: 'Grants IPNoticeHub application Admin role. Members can manage security and global settings.'
  mailEnabled: false
  securityEnabled: true
  mailNickname: 'ipnoticehub-admins'
  uniqueName: 'IPNoticeHub-App-Admins'
  
  // MEMBERSHIP ASSIGNMENT
  members: {
    relationships: [
      defaultAdminUser.id // Adding your default admin here
    ]
  }
}

@description('Group for standard users with read/write access but no administrative privileges.')
resource userGroup 'Microsoft.Graph/groups@v1.0' = {
  displayName: 'IPNoticeHub-App-Users'
  description: 'Grants standard application access. Members can perform daily operations.'
  mailEnabled: false
  securityEnabled: true
  mailNickname: 'ipnoticehub-users'
  uniqueName: 'IPNoticeHub-App-Users'

  // MEMBERSHIP ASSIGNMENT
  members: {
    relationships: [
      standardTestUser.id // Adding the new test user here
    ]
  }
}

output adminGroupId string = adminGroup.id
output userGroupId string = userGroup.id