/* RESOURCE MODULE: Entra ID Users
   PROJECT: IPNoticeHub
   DESCRIPTION: Defines the foundational user accounts for the IPNoticeHub ecosystem, 
                including the Break-Glass/Emergency account and the Standard Test User.
*/

extension microsoftGraph

@description('The initial password for the accounts. In a production CI/CD pipeline, this should be sourced from Azure Key Vault.')
@secure()
param initialPassword string

// --- EMERGENCY ACCESS ACCOUNT (Break-Glass) --- 
// Purpose: Critical administrative access if standard auth providers fail.

resource emergencyAdmin 'Microsoft.Graph/users@v1.0' = {
  displayName: 'EMERGENCY ACCESS ACCOUNT'
  mailNickname: 'emergency_admin'
  userPrincipalName: 'emergency_admin@everflowing555gmail.onmicrosoft.com'
  accountEnabled: true
  usageLocation: 'BG' // Recommended to set for license assignment later
  passwordProfile: {
    forceChangePasswordNextSignIn: true
    password: initialPassword
  }
}

// --- STANDARD TEST USER --- 
// Purpose: Verification of standard "App User" roles and non-privileged access.

resource testUser 'Microsoft.Graph/users@v1.0' = {
  displayName: 'Test User'
  mailNickname: 'Test-User'
  userPrincipalName: 'Test-User@ipnoticehub.com'
  accountEnabled: true
  usageLocation: 'BG'
  passwordProfile: {
    forceChangePasswordNextSignIn: true
    password: initialPassword
  }
}

output emergencyAdminId string = emergencyAdmin.id
output testUserId string = testUser.id