# ?? Instalación - Installation

## ?? Requisitos Previos

Antes de instalar BookingService.Api, asegúrate de tener:

### Obligatorio
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** - Versión 8.0 o superior
  - Verifica con: `dotnet --version`
  
- **Base de Datos** (elige una):
  - [SQL Server 2019+](https://www.microsoft.com/sql-server)
  - [PostgreSQL 12+](https://www.postgresql.org/)
  - [LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (para desarrollo local)

- **Git** - Para clonar el repositorio
  - Verifica con: `git --version`

### Recomendado
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** o **[Visual Studio Code](https://code.visualstudio.com/)**
- **[Postman](https://www.postman.com/downloads/)** o **[Insomnia](https://insomnia.rest/download)** - Para testear endpoints
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** - Para containerización

### Opcional
- **[SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** - Gestor de BD

---

## ?? Pasos de Instalación

### 1?? Clonar el Repositorio

```bash
git clone https://github.com/Migu66/BookingService.Api.git
cd BookingService.Api/BookingService.Api
```

### 2?? Verificar .NET Installation

```bash
dotnet --version
# Deberías ver algo como: 8.0.xxx
```

### 3?? Restaurar Dependencias

```bash
dotnet restore
```

Esto descargará todos los packages NuGet necesarios:
- Entity Framework Core
- MediatR
- FluentValidation
- AutoMapper
- Serilog
- JWT Bearer
- Swagger (Swashbuckle)

### 4?? Configurar Base de Datos

#### Opción A: SQL Server LocalDB (Recomendado para desarrollo)

```bash
# Connection string por defecto en appsettings.json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookingServiceDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

#### Opción B: SQL Server completo

```bash
"DefaultConnection": "Server=localhost;Database=BookingServiceDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### Opción C: PostgreSQL

```bash
"DefaultConnection": "Host=localhost;Port=5432;Database=BookingServiceDb;Username=postgres;Password=YourPassword;"
```

**Para más detalles:** Ver [Environment-Setup.md](03-Environment-Setup.md)

### 5?? Ejecutar Migraciones

```bash
# Navega al directorio del proyecto
cd BookingService.Api

# Crear la base de datos
dotnet ef database update
```

Si es la primera vez, esto:
- Crea la base de datos
- Crea todas las tablas
- Aplica todas las configuraciones

### 6?? Construir el Proyecto

```bash
dotnet build
```

Verifica que compila sin errores:
```
Build succeeded!
```

### 7?? Ejecutar la Aplicación

```bash
dotnet run
```

Deberías ver algo como:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5001
```

### 8?? Probar la API

Abre en tu navegador:
```
https://localhost:7001/swagger
```

Deberías ver **Swagger UI** con todos los endpoints documentados.

---

## ?? Instalación con Docker

### Requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado

### Pasos

#### 1. Construir la imagen

```bash
docker build -t bookingservice-api:latest .
```

#### 2. Ejecutar el contenedor

```bash
docker run -d `
  --name bookingservice `
  -p 8080:80 `
  -e ConnectionStrings__DefaultConnection="Server=sql-server;Database=BookingServiceDb;User Id=sa;Password=YourPassword;" `
  bookingservice-api:latest
```

#### 3. Acceder a la API

```
http://localhost:8080/swagger
```

### Con Docker Compose (Recomendado)

Crea un archivo `docker-compose.yml`:

```yaml
version: '3.8'

services:
  sql-server:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
    environment:
      ConnectionStrings__DefaultConnection: "Server=sql-server;Database=BookingServiceDb;User Id=sa;Password=YourPassword123!;"
    depends_on:
 - sql-server

volumes:
  mssql_data:
```

Ejecuta:
```bash
docker-compose up -d
```

Accede a: `http://localhost:8080/swagger`

---

## ? Verificación de Instalación

### Verificar que todo está correcto

```bash
# 1. Verificar .NET
dotnet --version

# 2. Verificar base de datos
dotnet ef migrations list

# 3. Construir el proyecto
dotnet build

# 4. Ejecutar tests (si existen)
dotnet test

# 5. Ejecutar la aplicación
dotnet run
```

Todos estos comandos deberían ejecutarse sin errores.

### Swagger accesible

Abre: `https://localhost:7001/swagger`

Si ves la interfaz de Swagger, ¡la instalación fue exitosa! ?

---

## ?? Troubleshooting

### Error: "SQL Server no está conectando"

**Solución:**
1. Verifica que SQL Server está corriendo
2. Verifica la connection string en `appsettings.json`
3. Para LocalDB:
   ```bash
   sqllocaldb info mssqllocaldb
   sqllocaldb start mssqllocaldb
   ```

### Error: "Migraciones no aplican"

**Solución:**
```bash
# Mostrar migraciones pending
dotnet ef migrations list

# Borrar base de datos y empezar de nuevo
dotnet ef database drop
dotnet ef database update
```

### Error: ".NET 8 SDK no encontrado"

**Solución:**
```bash
# Verifica versión instalada
dotnet --version

# Si no es 8.0, descárgalo desde:
https://dotnet.microsoft.com/download/dotnet/8.0
```

### Error: "Port 5001/7001 ya está en uso"

**Solución:**
```bash
# Cambia los puertos en launchSettings.json
# O mata el proceso:
netstat -ano | findstr :7001
taskkill /PID <PID> /F
```

### Error: "Swagger no carga"

**Solución:**
1. Verifica que la aplicación está corriendo: `https://localhost:7001`
2. Verifica que no hay errores en la consola
3. Limpia el cache del navegador

---

## ?? Checklist Post-Instalación

- [ ] .NET 8 SDK instalado (`dotnet --version` = 8.0.x)
- [ ] Git clonado correctamente
- [ ] `dotnet restore` ejecutado sin errores
- [ ] Base de datos configurada en `appsettings.json`
- [ ] `dotnet ef database update` ejecutado
- [ ] `dotnet build` compila sin errores
- [ ] `dotnet run` inicia sin errores
- [ ] Swagger carga en `https://localhost:7001/swagger`
- [ ] Puedes ver los endpoints en Swagger

---

## ?? ¡Instalación Completada!

Si todos los pasos fueron exitosos, estás listo para:

1. **Explorar la API** ? [Quick-Start.md](02-Quick-Start.md)
2. **Entender la arquitectura** ? [../02-Architecture/](../02-Architecture/)
3. **Empezar a desarrollar** ? [../03-Development/](../03-Development/)

---

## ?? Siguiente Paso

?? **[Quick-Start.md](02-Quick-Start.md)** - Haz tu primer request en 5 minutos
