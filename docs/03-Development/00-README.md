# ?? Desarrollo - Development Guide

## ?? Índice de Desarrollo

Esta sección es la **guía práctica** para **desarrollar features** en la aplicación.

### ?? Documentos:

1. **[00-README.md](00-README.md)** (Este)
   - Índice y navegación

2. **[01-Quick-Guide.md](01-Quick-Guide.md)**
   - ¿Dónde pongo cada cosa?
   - Checklist para nueva feature
   - Patrón de nombres
   - Errores comunes

3. **[02-Project-Structure.md](02-Project-Structure.md)**
   - Árbol de carpetas completo
   - Propósito de cada carpeta
   - Archivos importantes

4. **[03-Adding-Features.md](03-Adding-Features.md)**
   - Paso a paso: agregar una feature
   - Ejemplo: Calificaciones de recursos
   - Cada paso con código

5. **[04-Code-Examples.md](04-Code-Examples.md)**
   - Ejemplos de código real
   - Cómo escribir Commands
   - Cómo escribir Queries
   - Cómo escribir Validators
   - Cómo escribir Handlers
   - Cómo escribir Controllers

6. **[05-Best-Practices.md](05-Best-Practices.md)**
   - Patrones recomendados
   - Lo que SÍ hacer
   - Lo que NO hacer
- Tips y tricks

---

## ?? ¿Por Dónde Empezar?

### "Necesito agregar una funcionalidad AHORA"
?? **[01-Quick-Guide.md](01-Quick-Guide.md)**

### "Quiero entender cómo está organizado el código"
?? **[02-Project-Structure.md](02-Project-Structure.md)**

### "Quiero agregar una feature completa paso a paso"
?? **[03-Adding-Features.md](03-Adding-Features.md)**

### "Necesito ejemplos de código"
?? **[04-Code-Examples.md](04-Code-Examples.md)**

### "Quiero saber las mejores prácticas"
?? **[05-Best-Practices.md](05-Best-Practices.md)**

---

## ? Quick Links

| Necesito... | Documento |
|------------|-----------|
| Saber dónde va cada cosa | [01-Quick-Guide.md](01-Quick-Guide.md) |
| Ver el árbol de carpetas | [02-Project-Structure.md](02-Project-Structure.md) |
| Agregar una feature completa | [03-Adding-Features.md](03-Adding-Features.md) |
| Ejemplos de código | [04-Code-Examples.md](04-Code-Examples.md) |
| Mejores prácticas | [05-Best-Practices.md](05-Best-Practices.md) |

---

## ?? Tareas Comunes

### "Quiero crear un nuevo endpoint"

**Pasos:**
1. Crear Command/Query en `Core/Application/Features/{Feature}/`
2. Crear Validator
3. Crear Handler
4. Crear método en Controller

**Guía completa:** [03-Adding-Features.md](03-Adding-Features.md)

---

### "Necesito agregar validación"

**Pasos:**
1. Agregar Rule en el Validator
2. Mensaje descriptivo
3. Test la validación

**Guía:** [04-Code-Examples.md](04-Code-Examples.md) ? Validators

---

### "Quiero cambiar la BD"

**Pasos:**
1. Editar `Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`
2. Crear migración: `dotnet ef migrations add {Nombre}`
3. Actualizar BD: `dotnet ef database update`

**Guía:** [02-Project-Structure.md](02-Project-Structure.md) ? Infrastructure

---

### "Necesito crear una entidad nueva"

**Pasos:**
1. Crear `Core/Domain/Entities/{Entity}.cs`
2. Crear `Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`
3. Agregar `DbSet<{Entity}>` en `ApplicationDbContext`
4. Migración y update

**Guía:** [03-Adding-Features.md](03-Adding-Features.md)

---

## ?? Estructura Rápida

```
Core/
??? Domain/        ? Entidades, reglas
??? Application/   ? CQRS, lógica

Infrastructure/            ? BD, servicios

Presentation/          ? Controllers

docs/     ? Esta documentación
```

---

## ?? Flujo Típico de Desarrollo

```
1. Lee [Quick-Guide.md](01-Quick-Guide.md)
   ?
2. Planea la feature (¿Command? ¿Query? ¿Entidad nueva?)
   ?
3. Sigue [03-Adding-Features.md](03-Adding-Features.md)
   ?
4. Revisa [04-Code-Examples.md](04-Code-Examples.md) para código
   ?
5. Aplica [05-Best-Practices.md](05-Best-Practices.md)
   ?
6. ¡Deploy! ??
```

---

## ?? Conceptos Clave para Desarrollar

### CQRS
- **Command** = Modifica datos (Create, Update, Delete)
- **Query** = Solo lee (Get, List, Search)

### Layers
- **Domain** = Entidades y reglas
- **Application** = CQRS y lógica
- **Infrastructure** = BD y servicios
- **Presentation** = Controllers y HTTP

### Validation
- **Domain** = Reglas de negocio
- **Application** = FluentValidation
- **Presentation** = Authorization

### Patterns
- **CQRS** = Separación de lectura/escritura
- **Repository** = Abstracción de BD
- **Result** = Manejo de errores sin excepciones
- **DTO** = Desacoplamiento de entities

---

## ?? Checklist: Antes de Hacer Commit

- [ ] ¿Compiló sin errores? (`dotnet build`)
- [ ] ¿Pasa todos los tests? (`dotnet test`)
- [ ] ¿Sigue el patrón de la codebase?
- [ ] ¿Tiene XML comments? (para Swagger)
- [ ] ¿Validations en 3 capas?
- [ ] ¿DTOs correctos?
- [ ] ¿Nombres claros?
- [ ] ¿No hay código muerto?

---

## ?? Necesito Ayuda

| Problema | Solución |
|----------|----------|
| No sé dónde poner el código | [01-Quick-Guide.md](01-Quick-Guide.md) |
| No entiendo la estructura | [02-Project-Structure.md](02-Project-Structure.md) |
| Necesito un ejemplo | [04-Code-Examples.md](04-Code-Examples.md) |
| Cometí un error común | [05-Best-Practices.md](05-Best-Practices.md) |
| Quiero agregar una feature | [03-Adding-Features.md](03-Adding-Features.md) |

---

## ?? Flujo Recomendado de Aprendizaje

### Primera vez?
1. Lee [01-Quick-Guide.md](01-Quick-Guide.md)
2. Lee [02-Project-Structure.md](02-Project-Structure.md)
3. Lee [04-Code-Examples.md](04-Code-Examples.md)

### Voy a escribir código?
1. [01-Quick-Guide.md](01-Quick-Guide.md) - Checklist
2. [03-Adding-Features.md](03-Adding-Features.md) - Paso a paso
3. [04-Code-Examples.md](04-Code-Examples.md) - Copiar patrones

### Quiero escribir código bueno?
1. [04-Code-Examples.md](04-Code-Examples.md) - Ejemplos
2. [05-Best-Practices.md](05-Best-Practices.md) - Mejores prácticas

---

## ?? Principios de Desarrollo

? **Mantén Domain puro** - Sin referencias a EF Core  
? **Usa DTOs** - Nunca expongas entidades en API  
? **Valida en 3 capas** - Domain, Application, Presentation  
? **Controllers delgados** - Solo coordinan  
? **Nombres claros** - El código se explica solo  
? **Un Handler por operación** - CQRS puro  
? **Tests en mente** - Código testeable  

---

## ?? Ahora Sí, Elige un Documento

- ?? **[01-Quick-Guide.md](01-Quick-Guide.md)** - Si necesitas empezar YA
- ?? **[02-Project-Structure.md](02-Project-Structure.md)** - Si quieres entender todo
- ?? **[03-Adding-Features.md](03-Adding-Features.md)** - Si vas a agregar una feature
- ?? **[04-Code-Examples.md](04-Code-Examples.md)** - Si necesitas código
- ?? **[05-Best-Practices.md](05-Best-Practices.md)** - Si quieres escribir bien

---

**Listo? Vamos! ??**
