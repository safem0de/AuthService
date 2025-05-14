```bash
- Postgres
- Redis
```

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
# Postgres
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
# LDAP
dotnet add package System.DirectoryServices.Protocols
# JWT
dotnet add package System.IdentityModel.Tokens.Jwt
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

```bash
openssl rand -base64 32
```
* https://jwt.io/ (for token-local)

nerdctl run --rm -it cloudflare/cloudflared tunnel \
  --no-tls-verify \
  --url https://192.168.100.249:5068
