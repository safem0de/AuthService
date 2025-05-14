## Environment
```bash
- Postgres
- Redis
```

## Packages
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
---
## How to Run this Projects
```bash
dotnet watch run --urls="https://0.0.0.0:5068"
```
* migration
```bash
dotnet ef migrations add InitLocalAdmin
dotnet ef database update
```
* remove migration
```bash
rm -r Data/Migrations       # Mac or Linux
or
rmdir /s /q Data\Migrations # Windows
dotnet ef database drop
```
* Generate Random Token String
```bash
openssl rand -base64 32
```
* https://jwt.io/ (for token-local)
---
### cloudflared for test with Oracle NetSuite(Integration)
```bash
nerdctl run --rm -it cloudflare/cloudflared tunnel \
  --no-tls-verify \
  --url https://192.168.100.249:5068
```
