# Variables de Entorno

## Variables Requeridas

| Variable | Descripción | Ejemplo | Requerida |
|----------|-------------|---------|-----------|
| `ConnectionStrings__SqlServer` | Connection string SQL Server | `Server=...;Database=...;` | **Sí** |
| `Jwt__Key` | Clave secreta JWT (32+ chars) | `AbCdEfGhIjKlMnOpQrStUvWxYz123456` | **Sí** |
| `Jwt__Issuer` | Emisor token | `InstitutoTupacAmaru` | Sí |
| `Jwt__Audience` | Audiencia token | `InstitutoTupacAmaruAdmin` | Sí |
| `ASPNETCORE_ENVIRONMENT` | Ambiente actual | `Development` / `Production` | **Sí** |
| `ASPNETCORE_URLS` | URLs de escucha | `http://0.0.0.0:8080` | Sí (prod) |

## Variables Opcionales

| Variable | Default | Descripción |
|----------|---------|-------------|
| `Logging__LogLevel__Default` | `Information` | Nivel log global |
| `Logging__LogLevel__Microsoft__AspNetCore` | `Warning` | Log framework |
| `AllowedHosts` | `*` | Host headers permitidos |
| `Cors__AllowedOrigins` | `*` | Orígenes CORS (si se configura) |

---

## Por Ambiente

### Development (Local)
```bash
# .env (no commitear) o launchSettings.json
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5127
ConnectionStrings__SqlServer=Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;
Jwt__Key=Tupac@Amaru#Instituto!JWT$2026*Clave&MuySecreta=32chars
Jwt__Issuer=InstitutoTupacAmaru
Jwt__Audience=InstitutoTupacAmaruAdmin
```

### Staging
```bash
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__SqlServer=Server=staging-sql;Database=InstitutoDB;User Id=app_user;Password=StagingPass123;TrustServerCertificate=True;
Jwt__Key=StagingJwtKey32CharactersMinimum!!!
Jwt__Issuer=InstitutoTupacAmaru
Jwt__Audience=InstitutoTupacAmaruAdmin
Logging__LogLevel__Default=Warning
AllowedHosts=staging-api.tupac.edu
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__SqlServer=Server=prod-sql;Database=InstitutoDB;User Id=app_user;Password=ProdSuperSecretPass!!!;TrustServerCertificate=False;Encrypt=True;
Jwt__Key=ProductionJwtKeyMustBeVeryLongAndRandom32CharsMinimum
Jwt__Issuer=InstitutoTupacAmaru
Jwt__Audience=InstitutoTupacAmaruAdmin
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft__AspNetCore=Error
AllowedHosts=api.tupac.edu,www.tupac.edu
```

---

## Docker / Contenedores

### docker-compose.yml
```yaml
version: '3.8'
services:
  backend:
    build: ./backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://0.0.0.0:8080
      - ConnectionStrings__SqlServer=Server=sqlserver;Database=InstitutoDB;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True;
      - Jwt__Key=${JWT_KEY}
      - Jwt__Issuer=InstitutoTupacAmaru
      - Jwt__Audience=InstitutoTupacAmaruAdmin
    ports:
      - "8080:8080"
    depends_on:
      - sqlserver
    env_file:
      - .env.production

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

### .env.production (no commitear)
```env
SA_PASSWORD=SqlServerStrongPass123!
JWT_KEY=ProductionJwtKeyMustBeVeryLongAndRandom32CharsMinimum
```

---

## Kubernetes / Azure / AWS

### Azure App Service
- Settings → Configuration → Application settings
- Agregar cada variable como "New application setting"
- Connection strings en pestaña aparte ("Connection strings")

### AWS Elastic Beanstalk / ECS
- `.ebextensions/environment.config` o Task Definition environment variables

### Kubernetes
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: backend-secrets
type: Opaque
stringData:
  ConnectionStrings__SqlServer: "Server=...;Password=...;"
  Jwt__Key: "ProductionJwtKey..."
---
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
      - name: backend
        envFrom:
        - secretRef:
            name: backend-secrets
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://0.0.0.0:8080"
```

---

## Validación en Inicio (Program.cs)

```csharp
var requiredVars = new[]
{
    "ConnectionStrings__SqlServer",
    "Jwt__Key",
    "Jwt__Issuer",
    "Jwt__Audience"
};

foreach (var varName in requiredVars)
{
    var value = builder.Configuration[varName];
    if (string.IsNullOrEmpty(value))
        throw new InvalidOperationException($"Variable de entorno requerida no configurada: {varName}");
}

// Validación extra Jwt:Key length
var jwtKey = builder.Configuration["Jwt__Key"]!;
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");
```

---

## Seguridad

| Regla | Implementación |
|-------|----------------|
| **Nunca en código** | No hardcodear secrets en C# |
| **Nunca en Git** | `.gitignore` excluye `.env`, `appsettings.*.json` |
| **Rotación** | Cambiar `Jwt__Key` invalida tokens existentes (planificar) |
| **Mínimo privilegio** | SQL user con permisos solo `InstitutoDB` |
| **Encriptación en tránsito** | `Encrypt=True` en connection string prod |
| **Auditoría** | Log acceso a secrets (Key Vault, AWS Secrets Manager) |