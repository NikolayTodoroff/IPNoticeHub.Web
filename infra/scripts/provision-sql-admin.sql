/*
Project: IPNoticeHub
Description: Provision Entra ID Admin User and Assign DB_OWNER Permissions
Environment: LAB
Owner: Nikolay
*/

PRINT 'Processing permissions for Entra User: sql-admin@ipnoticehub.com';

IF NOT EXISTS (
    SELECT 1 
    FROM sys.database_principals 
    WHERE name = 'sql-admin@ipnoticehub.com'
)
BEGIN
    PRINT 'User does not exist. Creating.';
    CREATE USER [sql-admin@ipnoticehub.com] FROM EXTERNAL PROVIDER;
END
ELSE
BEGIN
    PRINT 'User already exists in database.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r 
        ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals u
        ON drm.member_principal_id = u.principal_id
    WHERE r.name = 'db_owner'
      AND u.name = 'sql-admin@ipnoticehub.com'
)
BEGIN
    ALTER ROLE db_owner ADD MEMBER [sql-admin@ipnoticehub.com];
END
GO

PRINT 'Admin setup complete.';