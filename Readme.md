```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
dotnet add package System.DirectoryServices.Protocols
```

```bash
dotnet watch run
```

```bash
dotnet ef migrations add InitLocalAdmin
dotnet ef database update
```

```bash
rm -r Data/Migrations       # Mac or Linux
or
rmdir /s /q Data\Migrations # Windows
dotnet ef database drop
```