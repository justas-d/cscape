The SQLite db isn't quite plug and play. You'll have to create the tables yourself or let the dotnet ef tooling do it for you:

1. `dotnet restore` and then `dotnet build` the project. Make sure no errors sneak up during the process.
2. `dotnet ef migrations add MyFirstMigration` to create a migration that holds all required tables.
3. `dotnet ef database update MyFirstMigration` to apply the migration we made in the previous step.

You should hopefuly be good now.


