SELECT TOP (1000) [Id]
      ,[FullName]
      ,[Email]
      ,[PasswordHash]
      ,[RoleId]
      ,[IsGoogleAccount]
      ,[CreatedAt]
  FROM [LibraryManagementDb].[dbo].[Users]
    use LibraryManagementDb
	ALTER TABLE Users
DROP COLUMN IsGoogleAccount;
ALTER TABLE Users
DROP CONSTRAINT DF__Users__IsGoogleA__4CA06362;