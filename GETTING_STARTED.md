# ?? Instrucciones de Inicio - BookingService API

## ?? Pasos para ejecutar el proyecto

### **1. Verificar Requisitos Previos**

Asegúrate de tener instalado:
- ? .NET 8 SDK
- ? SQL Server (LocalDB o instancia completa)
- ? Visual Studio 2022 / VS Code / Rider

### **2. Configurar la Base de Datos**

#### **Opción A: Usando Package Manager Console (Visual Studio)**

```powershell
Add-Migration InitialCreate
Update-Database
```

#### **Opción B: Usando dotnet CLI**

```bash
# Desde la carpeta raíz del proyecto
dotnet ef migrations add InitialCreate --project BookingService.Api
dotnet ef database update --project BookingService.Api
```

Si `dotnet ef` no está instalado:
```bash
dotnet tool install --global dotnet-ef
```

### **3. Verificar la Cadena de Conexión**

Abre `appsettings.json` o `appsettings.Development.json` y ajusta la cadena de conexión según tu configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookingServiceDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Opciones comunes:**

- **LocalDB**: `Server=(localdb)\\mssqllocaldb;Database=BookingServiceDb;Trusted_Connection=True;`
- **SQL Server con autenticación Windows**: `Server=localhost;Database=BookingServiceDb;Trusted_Connection=True;TrustServerCertificate=True`
- **SQL Server con usuario/password**: `Server=localhost;Database=BookingServiceDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True`

### **4. Ejecutar la Aplicación**

```bash
dotnet run --project BookingService.Api
```

O desde Visual Studio: presiona **F5** o **Ctrl+F5**

### **5. Acceder a Swagger UI**

Una vez iniciada la aplicación, abre tu navegador en:

```
https://localhost:7xxx
```

(El puerto exacto se mostrará en la consola al iniciar)

---

## ?? Primeros Pasos - Uso de la API

### **1. Registrar un Usuario**

**Endpoint:** `POST /api/auth/register`

```json
{
  "name": "Admin User",
  "email": "admin@bookingservice.com",
  "password": "Admin123!"
}
```

### **2. Promover a Administrador (Manualmente en la BD)**

Por ahora, el primer usuario se crea como **User**. Para convertirlo en **Admin**, ejecuta este SQL:

```sql
UPDATE Users SET Role = 1 WHERE Email = 'admin@bookingservice.com';
```

**Valores de Role:**
- `0` = User
- `1` = Admin

### **3. Iniciar Sesión**

**Endpoint:** `POST /api/auth/login`

```json
{
  "email": "admin@bookingservice.com",
  "password": "Admin123!"
}
```

Copia el `token` de la respuesta.

### **4. Autenticarte en Swagger**

1. Haz clic en el botón **"Authorize"** (candado) en Swagger UI
2. Ingresa: `Bearer {tu-token-aquí}`
3. Haz clic en **Authorize**

Ahora puedes probar todos los endpoints protegidos.

---

## ?? Estructura de la Base de Datos

La migración creará las siguientes tablas:

- **Users** - Usuarios del sistema con roles
- **Resources** - Recursos reservables
- **Reservations** - Reservas de recursos
- **BlockedTimes** - Períodos bloqueados para mantenimiento

---

## ?? Endpoints Principales

### Autenticación (Sin autenticación requerida)
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/login` - Iniciar sesión

### Recursos (Autenticación requerida)
- `GET /api/resources` - Listar recursos
- `GET /api/resources/{id}` - Ver recurso
- `POST /api/resources` - Crear recurso (**Admin**)
- `PUT /api/resources/{id}` - Actualizar recurso (**Admin**)
- `DELETE /api/resources/{id}` - Eliminar recurso (**Admin**)

### Reservas (Autenticación requerida)
- `POST /api/reservations` - Crear reserva
- `GET /api/reservations/my` - Mis reservas
- `GET /api/reservations/{id}` - Ver reserva
- `DELETE /api/reservations/{id}` - Cancelar reserva
- `POST /api/reservations/availability` - Consultar disponibilidad
- `GET /api/reservations` - Todas las reservas (**Admin**)

### Bloqueos (**Solo Admin**)
- `GET /api/blockedtimes/resource/{resourceId}` - Ver bloqueos
- `POST /api/blockedtimes` - Crear bloqueo
- `DELETE /api/blockedtimes/{id}` - Eliminar bloqueo

---

## ?? Solución de Problemas

### **Error: "Cannot open database"**
- Verifica que SQL Server esté corriendo
- Comprueba la cadena de conexión en `appsettings.json`

### **Error: "dotnet ef not found"**
```bash
dotnet tool install --global dotnet-ef
```

### **Error al aplicar migraciones**
```bash
# Elimina la carpeta Migrations y vuelve a intentar
dotnet ef migrations add InitialCreate --project BookingService.Api
dotnet ef database update --project BookingService.Api
```

### **Logs no aparecen**
Los logs se guardan en la carpeta `BookingService.Api/logs/`

---

## ?? Siguiente Paso Recomendado

Después de levantar la API, considera:

1. ? Crear datos de seed (usuarios y recursos de prueba)
2. ? Agregar tests unitarios
3. ? Implementar refresh tokens
4. ? Configurar CI/CD
5. ? Dockerizar la aplicación

---

**¿Necesitas ayuda?** Revisa el archivo `README.md` para más detalles sobre la arquitectura y características del proyecto.
