# Sistema de Calificación por Puntos

Sistema web desarrollado en ASP.NET Core MVC con Razor Pages para la gestión de puntos acumulados, redención de premios y administración de premios.

## Características

### 1. Puntos Acumulados
- Visualización de puntos agrupados por número de documento
- Muestra total de ideas, total de puntos, puntos disponibles y puntos utilizados
- Detalle de ideas por usuario

### 2. Redención de Puntos
- Búsqueda de usuario por número de documento
- Visualización de puntos acumulados y disponibles
- Redención de premios con validación de puntos suficientes
- Listado de premios redimidos por usuario
- Validación automática: no permite redimir si no hay puntos suficientes

### 3. Tabla de Premios
- CRUD completo (Crear, Leer, Actualizar, Eliminar)
- Gestión de stock
- Activación/desactivación de premios
- Visualización de todos los premios disponibles

### 4. Datos de Prueba
- Generación de ideas de prueba
- Generación de premios de prueba
- Generación de redenciones de prueba
- Generación masiva de todos los datos

## Estructura del Proyecto

```
CalificacionXPuntosWeb/
├── Data/              # Datos (si se requiere persistencia)
├── Models/            # Modelos de datos
│   ├── Idea.cs
│   ├── Premio.cs
│   ├── Redencion.cs
│   └── PuntosAcumulados.cs
├── Views/             # Vistas MVC Razor
│   ├── Home/
│   │   ├── Index.cshtml
│   │   ├── PuntosAcumulados.cshtml
│   │   ├── RedencionPuntos.cshtml
│   │   ├── TablaPremios.cshtml
│   │   └── ...
│   └── Shared/
│       └── _Layout.cshtml
├── Services/          # Servicios de negocio
│   ├── IdeaService.cs
│   ├── PremioService.cs
│   └── RedencionService.cs
├── Shared/            # Componentes compartidos
│   ├── MainLayout.razor
│   └── NavMenu.razor
└── wwwroot/           # Archivos estáticos
    ├── css/
    └── img/
```

## Requisitos

- .NET 8.0 SDK o superior
- Visual Studio 2022 o Visual Studio Code

## Instalación y Ejecución

1. Clonar o descargar el proyecto
2. Abrir el proyecto en Visual Studio o desde la línea de comandos
3. Restaurar las dependencias:
   ```bash
   dotnet restore
   ```
4. Ejecutar el proyecto:
   ```bash
   dotnet run
   ```
5. Abrir el navegador en `https://localhost:5001` o `http://localhost:5000`

## Uso

### Generar Datos de Prueba

1. Navegar a "Datos de Prueba" en el menú
2. Generar ideas, premios y redenciones según necesidad
3. O usar "Generar Todo" para crear un conjunto completo de datos

### Consultar Puntos Acumulados

1. Navegar a "Puntos Acumulados"
2. Ver el listado de usuarios con sus puntos agrupados
3. Hacer clic en "Ver Detalle" para ver las ideas individuales

### Redimir Puntos

1. Navegar a "Redención Puntos"
2. Ingresar el número de documento del usuario
3. Seleccionar un premio disponible
4. El sistema validará automáticamente si hay puntos suficientes

### Administrar Premios

1. Navegar a "Tabla Premios"
2. Crear, editar o eliminar premios
3. Gestionar stock y estado de activación

## Tecnologías Utilizadas

- ASP.NET Core MVC
- Razor Pages
- .NET 8.0
- Bootstrap (para estilos)
- C# 12
- Entity Framework Core

## Notas

- Los datos se almacenan en memoria (no hay persistencia en base de datos)
- Al reiniciar la aplicación, los datos se perderán
- Para producción, se recomienda implementar persistencia con Entity Framework Core o similar

