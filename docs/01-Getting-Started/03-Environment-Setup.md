# ?? Configuración de Entorno - Environment Setup

## ?? Resumen Rápido

Este documento explica cómo configurar:
- **Base de datos** (SQL Server / PostgreSQL)
- **JWT** (Autenticación)
- **Logging** (Serilog)
- **Variables de entorno**

---

## ??? Configuración de Base de Datos

### Archivo Principal: `appsettings.json`

Este archivo contiene toda la configuración. Está en la raíz del proyecto:

```
BookingService.Api/
??? appsettings.json
```

### Connection Strings

#### SQL Server LocalDB (Desarrollo)

Perfecto para desarrollo local. No necesita instalación especial.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookingServiceDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Ventajas:** Fácil, sin contraseña, automático  
**Desventajas:** Solo Windows, solo desarrollo

#### SQL Server Completo

Para entornos de producción o testing.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BookingServiceDb;User Id=sa;Password=YourSecurePassword123!;TrustServerCertificate=True;"
  }
}
```

**Pasos:**
1. Instala [SQL Server 2019+](https://www.microsoft.com/sql-server)
2. Instala [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms)
3. Copia la connection string arriba
4. Reemplaza:
   - `YourSecurePassword123!` ? Tu password

#### PostgreSQL

Para desarrollo multiplataforma.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BookingServiceDb;Username=postgres;Password=YourPassword;"
  }
}
```

**Pasos:**
1. Instala [PostgreSQL](https://www.postgresql.org/download/)
2. Durante la instalación, **recuerda el password de `postgres`**
3. Copia la connection string arriba
4. Reemplaza `YourPassword` con tu password

#### Docker con Docker Compose

Para un entorno completo aislado.

Crea `docker-compose.yml` en la raíz:

```yaml
version: '3.8'

services:
  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql

volumes:
  mssql_data:
```

Connection string:
```json
"DefaultConnection": "Server=localhost,1433;Database=BookingServiceDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
```

Ejecuta:
```bash
docker-compose up -d
```

---

## ?? Configuración JWT

JWT es para autenticación con tokens. Configúralo en `appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "TuClaveSecretaMuyLargaDeAlMenos32Caracteres!@#$%^&*()",
  "Issuer": "BookingService.Api",
"Audience": "BookingService.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Explicación de cada campo:

| Campo | Valor | Ejemplo |
|-------|-------|---------|
| **Secret** | Clave secreta (mínimo 32 caracteres) | `"TuClaveSecretaMuyLargaDeAlMenos32Caracteres!@#$%^&*()"` |
| **Issuer** | Quién crea el token | `"BookingService.Api"` |
| **Audience** | Quién puede usar el token | `"BookingService.Client"` |
| **ExpirationMinutes** | Minutos antes de expirar | `60` = 1 hora |
| **RefreshTokenExpirationDays** | Días antes de expirar refresh token | `7` = 1 semana |

### ?? Seguridad

**NUNCA hardcodees secrets en el código.** Usa User Secrets en desarrollo.

---

## ?? User Secrets (Desarrollo Seguro)

Para no exponer secrets en git, usa User Secrets:

### Configurar User Secrets

```bash
# En el directorio del proyecto
cd BookingService.Api

# Inicializar User Secrets
dotnet user-secrets init

# Establecer secret
dotnet user-secrets set "JwtSettings:Secret" "TuClaveSecretaMuyLargaDeAlMenos32Caracteres!@#$%^&*()"
```

### Archivo de User Secrets

Los secrets se guardan en:
```
Windows: %APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json
Mac/Linux: ~/.microsoft/usersecrets/{UserSecretsId}/secrets.json
```

### Usar User Secrets en appsettings.json

En tu `Program.cs`:

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// En desarrollo, carga User Secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Resto de configuración...
```

---

## ?? appsettings.json Completo

Aquí está un ejemplo completo de configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookingServiceDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "TuClaveSecretaMuyLargaDeAlMenos32Caracteres!@#$%^&*()",
    "Issuer": "BookingService.Api",
    "Audience": "BookingService.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
   "Microsoft": "Warning",
   "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
      "Args": {
 "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console"
        }
      },
  {
        "Name": "File",
        "Args": {
        "path": "logs/log-.txt",
     "rollingInterval": "Day",
  "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

---

## ?? Archivos de Configuración por Entorno

ASP.NET Core soporta múltiples archivos de configuración:

```
BookingService.Api/
??? appsettings.json? Base
??? appsettings.Development.json      ? Desarrollo
??? appsettings.Staging.json          ? Staging
??? appsettings.Production.json       ? Producción
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Warning"
  }
}
```

### Ejecutar en diferentes entornos

```bash
# Desarrollo (por defecto)
dotnet run

# Staging
dotnet run --environment Staging

# Producción
dotnet run --environment Production
```

---

## ?? Configuración de Logging (Serilog)

Serilog es un logger estructurado. Configúralo en `appsettings.json`:

### Niveles de Log

| Nivel | Uso | Ejemplo |
|-------|-----|---------|
| **Verbose** | Máximo detalle (raro) | Variables internas |
| **Debug** | Debugging (desarrollo) | Entrada/salida de funciones |
| **Information** | Información general (producción) | Requests recibidos |
| **Warning** | Advertencias | Deprecated, fallbacks |
| **Error** | Errores | Excepciones |
| **Fatal** | Errores críticos | BD down, sin memoria |

### Ejemplos de Configuración

**Solo consola:**
```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
    { "Name": "Console" }
  ]
}
```

**Consola + Archivo:**
```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
 { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day"
      }
 }
  ]
}
```

---

## ?? Variables de Entorno

Para aplicaciones containerizadas o en producción, usa variables de entorno:

### Mapeo de appsettings ? Environment Variables

```bash
# appsettings.json
ConnectionStrings:DefaultConnection

# Environment Variable
ConnectionStrings__DefaultConnection
# (dobles guiones bajos para anidación)
```

### Ejemplos

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=..."
$env:JwtSettings__Secret = "..."

# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=..."
export JwtSettings__Secret="..."

# Docker
docker run -e ConnectionStrings__DefaultConnection="Server=..." myapp
```

---

## ? Checklist de Configuración

- [ ] `appsettings.json` - Connection string correcta
- [ ] BD accesible con la connection string
- [ ] `dotnet ef database update` - Sin errores
- [ ] User Secrets configurado (desarrollo)
- [ ] JWT Secret configurado (mínimo 32 caracteres)
- [ ] Logging configurado
- [ ] `dotnet build` - Compila sin errores
- [ ] `dotnet run` - Inicia sin errores

---

## ?? Troubleshooting

### "Connection string invalid"
Verifica:
- Nombre de servidor correcto
- Nombre de BD correcto
- Credentials correctos
- SQL Server está corriendo

### "Entity Framework migrations fail"
```bash
dotnet ef database drop
dotnet ef database update
```

### "JWT Secret too short"
Secret debe tener mínimo 32 caracteres.

### "Logging no aparece"
Verifica `LogLevel` en appsettings.json

---

## ?? Próximos Pasos

- [02-Quick-Start.md](02-Quick-Start.md) - Ejecuta la API
- [../02-Architecture/](../02-Architecture/) - Entiende la arquitectura
- [../03-Development/](../03-Development/) - Desarrolla features

---

**Pregunta:** ¿Cuál es la configuración más segura?
**Respuesta:** User Secrets en desarrollo, variables de entorno en producción.
