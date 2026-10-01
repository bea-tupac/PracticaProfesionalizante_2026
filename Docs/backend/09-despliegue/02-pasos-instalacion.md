# Pasos de Instalación

## 1. Clonar Repositorio

```bash
git clone <url-repositorio>
cd SitioWebInstituto/instituto
```

## 2. Base de Datos

> **Nota**: El archivo `CreateDatabase.sql` **no existe en el repo**. Usar el DDL documentado en:
> - `docs/backend/02-base-de-datos/02-script-creacion.md`
> - `docs/DATABASE.md`

### Ejecutar DDL (Script Documentado)

**Opción A: SSMS (GUI)**
1. Abrir SQL Server Management Studio
2. Conectar a: `localhost` (SQL Auth: `instituto_user` / `Instituto2026`) o `(localdb)\MSSQLLocalDB` (Windows Auth)
3. Nueva consulta → Pegar DDL de `docs/backend/02-base-de-datos/02-script-creacion.md`
4. Ejecutar (F5)

**Opción B: Línea de comandos (LocalDB)**
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i create_instituto_db.sql
```

**Opción C: sqlcmd con SQL Auth**
```bash
sqlcmd -S localhost -U instituto_user -P "Instituto2026" -i create_instituto_db.sql
```

**Verificación**:
```sql
USE InstitutoDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
-- Debe retornar: Administradores, Alumnos, Carreras, Formularios, Profesores
```

## 3. Configuración Backend

### Development (Local)
```bash
cd Instituto.API

# Opción A: User Secrets (recomendado para LocalDB)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "TuClaveDesarrollo32CharsMinimo!!!"

# Opción B: appsettings.Development.json (ya existe con SQL Auth para localhost)
# Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;
# No tocar si usas SQL Server local con usuario instituto_user
```

### Production
```bash
# Variables de entorno (ver 02-variables-entorno.md)
export ConnectionStrings__SqlServer="Server=prod-sql;Database=InstitutoDB;User Id=app;Password=...;"
export Jwt__Key="ProduccionJwtKey32CharsMinimum!!!"
export Jwt__Issuer="InstitutoTupacAmaru"
export Jwt__Audience="InstitutoTupacAmaruAdmin"
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:8080"
```

## 4. Restaurar Dependencias y Compilar

```bash
cd Instituto.API
dotnet restore
dotnet build --configuration Release
```

## 5. Crear Primer Administrador (Solo Primera Vez, Solo Development)

```bash
# Asegurar que backend esté corriendo en Development
cd Instituto.API
dotnet run --environment Development &
# O en otra terminal

# Esperar a "Now listening on: http://localhost:5127"

# Crear admin (endpoint solo disponible en Development)
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Super",
    "apellido": "Admin",
    "email": "admin@tupac.edu.ar",
    "password": "Tupac123",
    "role": "SuperAdmin"
  }'

# Respuesta esperada:
# {"isSuccess":true,"message":"Operación exitosa","data":"Administrador 'admin@tupac.edu.ar' creado correctamente. Este endpoint ya no puede volver a usarse."}
```

## 6. Verificar Login

```bash
curl -X POST http://localhost:5127/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@tupac.edu.ar","password":"Tupac123"}'

# Respuesta (ApiResponse<LoginResponse>):
# {"isSuccess":true,"message":"Operación exitosa","data":{"token":"...","expiraEn":"...","admin":{"id":1,"nombre":"Super","apellido":"Admin","email":"admin@tupac.edu.ar","role":"SuperAdmin"}}}
```

## 7. Probar Endpoints CRUD

```bash
# Guardar token (extraer del login anterior)
TOKEN="<token-del-login>"

# Carreras (público GET)
curl http://localhost:5127/api/carreras

# Alumnos (auth)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5127/api/alumnos

# Crear carrera (auth)
curl -X POST http://localhost:5127/api/carreras \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Tecnicatura en IA","duracionAnios":3,"estado":"Activa"}'

# Listado (auth)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5127/api/listado
```

---

## 8. Frontend (Vue 3) - Instalación Paralela

```bash
cd ../Frontend
npm install
npm run dev
# Abre http://localhost:5176
```

**No hay proxy Vite** - El frontend usa `fetch` directo a `VITE_API_URL=http://localhost:5127` (configurado en `.env`):
```env
VITE_API_URL=http://localhost:5127
```

---

## 9. Publicar para Producción

### Backend - Self-Contained
```bash
cd Instituto.API
dotnet publish -c Release -o ./publish --self-contained false
# Copiar carpeta publish/ al servidor
```

### Backend - Framework-Dependent (Recomendado si .NET instalado en servidor)
```bash
dotnet publish -c Release -o ./publish
```

### Frontend - Build Estático
```bash
cd ../Frontend
npm run build
# Copiar carpeta dist/ a servidor web (nginx/IIS/Apache)
```

---

## 10. Configuración Servidor Producción (Linux/Windows)

### systemd Service (Linux)
```ini
# /etc/systemd/system/instituto-backend.service
[Unit]
Description=Instituto Backend API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/instituto/Instituto.API
ExecStart=/usr/bin/dotnet /var/www/instituto/Instituto.API/Instituto.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=instituto-backend
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
# Variables de entorno desde /etc/environment o EnvironmentFile

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable instituto-backend
sudo systemctl start instituto-backend
sudo systemctl status instituto-backend
```

### IIS (Windows)
1. Instalar **ASP.NET Core Hosting Bundle**
2. Crear Application Pool: "No Managed Code", Integrated
3. Sitio Web → Physical Path: `C:\inetpub\instituto\backend\publish`
4. Configurar `web.config` (generado por `dotnet publish`)
5. Variables de entorno en **Advanced Settings** → **Environment Variables**

### Nginx Reverse Proxy (Linux)
```nginx
# /etc/nginx/sites-available/instituto-api
server {
    listen 80;
    server_name api.tupac.edu;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/instituto-api /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

---

## Checklist Post-Instalación

- [ ] BD creada y accesible
- [ ] Backend compila sin errores
- [ ] Primer admin creado (`/api/setup/admin`)
- [ ] Login funciona (`/api/auth/login`)
- [ ] JWT token válido (8hs expiración)
- [ ] Endpoints CRUD responden 200/201
- [ ] CORS permite frontend
- [ ] Logs visibles (Console / File)
- [ ] Health check `/health` responde 200 (si configurado)
- [ ] Frontend conecta a backend correctamente