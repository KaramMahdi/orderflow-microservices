-- Creates the SQL login the application uses instead of sa. Safe to run again.
-- Run by the sqlserver-init service in docker-compose.yml as sa.
-- Scripting variables (sqlcmd reads them from environment variables of the same name):
--   AppLogin     login name (SQL_APP_LOGIN in .env)
--   AppPassword  login password (SQL_APP_PASSWORD in .env); must not contain a single quote
--
-- Permissions: dbcreator, so the login can create the OrderFlow database and then owns it
-- (the DbMigrator creates it in Milestone 5). No sysadmin.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @login sysname = N'$(AppLogin)';
DECLARE @password nvarchar(128) = N'$(AppPassword)';
DECLARE @statement nvarchar(max);

IF SUSER_ID(@login) IS NULL
BEGIN
    SET @statement = N'CREATE LOGIN ' + QUOTENAME(@login)
        + N' WITH PASSWORD = ' + QUOTENAME(@password, N'''')
        + N', DEFAULT_DATABASE = [master], CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;';
    EXEC sys.sp_executesql @statement;
    PRINT N'Login ' + @login + N' created.';
END
ELSE
BEGIN
    -- Keep the password in line with .env.
    SET @statement = N'ALTER LOGIN ' + QUOTENAME(@login)
        + N' WITH PASSWORD = ' + QUOTENAME(@password, N'''') + N';';
    EXEC sys.sp_executesql @statement;
    PRINT N'Login ' + @login + N' already exists; password updated.';
END;

IF IS_SRVROLEMEMBER(N'dbcreator', @login) = 0
BEGIN
    SET @statement = N'ALTER SERVER ROLE [dbcreator] ADD MEMBER ' + QUOTENAME(@login) + N';';
    EXEC sys.sp_executesql @statement;
    PRINT N'Login ' + @login + N' added to dbcreator.';
END;
