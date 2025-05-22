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
rm -r Migrations       # Mac or Linux
or
remove folder Migrations # Windows
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

### OAuth1.0 (NetSuite)
```bash
| Field                | Value            |
| -------------------- | ---------------- |
| Consumer Key         | จาก Integration  |
| Consumer Secret      | จาก Integration  |
| Token                | จาก Access Token |
| Token Secret         | จาก Access Token |
| Signature Method     | HMAC-SHA256      |
| Add params to header | Yes              |
```

## Example SuiteQL
```bash
## customer
{
    "q": "SELECT TOP 10 * FROM customer"
}
## employee
{
    "q": "SELECT TOP 10 * FROM employee"
}
## partner
{
    "q": "SELECT TOP 10 * FROM partner"
}
```