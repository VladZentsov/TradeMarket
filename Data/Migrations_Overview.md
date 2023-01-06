# Migrations Overview

## Preparation

To get started, you need to create an empty ApplicationDbContext constructor. Each DbContext instance must be configured to use one and only one database provider. So you need to add the following lines of code:

```
public ApplicationDbContext()
{
}
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TradeMarket;Trusted_Connection=True;");
}
```

**After you create the migration and database, be sure to delete this code before running the tests**

## Create your first migration

You're now ready to add your first migration! Instruct EF Core to create a migration named InitialCreate:

---

.NET Core CLI
```
dotnet ef migrations add InitialCreate
```

---

Visual Studio
```
Add-Migration InitialCreate
```

---

EF Core will create a directory called Migrations in your project, and generate some files.

## Create your database and schema

At this point you can have EF create your database and create your schema from the migration. This can be done via the following:

---

.NET Core CLI
```
dotnet ef database update
```

---

Visual Studio
```
Update-Database
```

---

That's all there is to it - your application is ready to run on your new database, and you didn't need to write a single line of SQL.
