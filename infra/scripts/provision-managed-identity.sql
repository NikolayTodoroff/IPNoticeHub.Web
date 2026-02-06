/*
Project: IPNoticeHub
Description: Provision Entra Managed Identity and Assign Permissions
Environment: LAB
Owner: Nikolay
*/

PRINT 'Setting up permissions for: ipnoticehub-web-lab';

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = 'ipnoticehub-web-lab'
)
BEGIN
    PRINT 'Creating user FROM EXTERNAL PROVIDER...';
    CREATE USER [ipnoticehub-web-lab] FROM EXTERNAL PROVIDER WITH DEFAULT_SCHEMA = dbo;
END
ELSE
BEGIN
    PRINT 'User already exists, skipping creation.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals u ON drm.member_principal_id = u.principal_id
    WHERE r.name = 'db_datareader'
      AND u.name = 'ipnoticehub-web-lab'
)
BEGIN
    PRINT 'Adding to db_datareader.';
    ALTER ROLE db_datareader ADD MEMBER [ipnoticehub-web-lab];
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals u ON drm.member_principal_id = u.principal_id
    WHERE r.name = 'db_datawriter'
      AND u.name = 'ipnoticehub-web-lab'
)
BEGIN
    PRINT 'Adding to db_datawriter.';
    ALTER ROLE db_datawriter ADD MEMBER [ipnoticehub-web-lab];
END
GO

PRINT 'Done.';
