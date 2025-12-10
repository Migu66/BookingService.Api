# Implementación de Paginación

## Resumen
Se ha implementado paginación para todos los métodos `GetAll()` en el API de BookingService.

## Archivos Creados

### 1. **PagedResult.cs** 
`BookingService.Api\Core\Application\Common\Models\PagedResult.cs`

Clase genérica que encapsula los resultados paginados:
- `Items`: Lista de elementos de la página actual
- `PageNumber`: Número de página actual
- `PageSize`: Tamaño de la página
- `TotalCount`: Total de elementos
- `TotalPages`: Total de páginas (calculado)
- `HasPreviousPage`: Indica si hay página anterior
- `HasNextPage`: Indica si hay página siguiente

### 2. **PaginationParameters.cs**
`BookingService.Api\Core\Application\Common\Models\PaginationParameters.cs`

Clase para parámetros de paginación (disponible para uso futuro):
- `PageNumber`: Número de página (por defecto 1)
- `PageSize`: Tamaño de página (por defecto 10, máximo 100)

## Archivos Modificados

### 1. **GetAllResourcesQuery.cs**
- ? Query ahora acepta `PageNumber` y `PageSize` como parámetros
- ? Retorna `PagedResult<ResourceDto>` en lugar de `List<ResourceDto>`
- ? Implementa lógica de paginación con `Skip` y `Take`
- ? Calcula el total de registros

### 2. **GetAllReservationsQuery.cs**
- ? Query ahora acepta `PageNumber` y `PageSize` como parámetros
- ? Retorna `PagedResult<ReservationDto>` en lugar de `List<ReservationDto>`
- ? Implementa lógica de paginación con `Skip` y `Take`
- ? Calcula el total de registros

### 3. **ResourcesController.cs**
- ? Método `GetAll()` acepta parámetros de query `pageNumber` y `pageSize`
- ? Valores por defecto: pageNumber=1, pageSize=10
- ? Retorna `PagedResult<ResourceDto>`
- ? Documentación XML actualizada

### 4. **ReservationsController.cs**
- ? Método `GetAll()` acepta parámetros de query `pageNumber` y `pageSize`
- ? Valores por defecto: pageNumber=1, pageSize=10
- ? Retorna `PagedResult<ReservationDto>`
- ? Documentación XML actualizada

## Uso de la API

### Obtener recursos con paginación
```http
GET /api/resources?pageNumber=1&pageSize=10
```

**Respuesta:**
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Obtener reservaciones con paginación (Admin)
```http
GET /api/reservations?pageNumber=2&pageSize=20
```

**Respuesta:**
```json
{
  "items": [...],
  "pageNumber": 2,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8,
  "hasPreviousPage": true,
  "hasNextPage": true
}
```

## Características

? **Paginación eficiente**: Usa `Skip` y `Take` para optimizar consultas a la base de datos
? **Metadata completa**: Incluye información de navegación entre páginas
? **Valores por defecto**: PageNumber=1, PageSize=10
? **Límite máximo**: PageSize máximo de 100 (configurable en `PaginationParameters`)
? **Retrocompatibilidad**: Si no se especifican parámetros, usa valores por defecto
? **Type-safe**: Usa generics para mantener el tipado fuerte
? **Swagger compatible**: Los parámetros aparecerán automáticamente en la documentación

## Compilación
? El proyecto compila correctamente sin errores
