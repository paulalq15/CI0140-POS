# CI0140-POS — Sistema de punto de venta para cafetería

Proyecto del curso **CI-0140 Calidad de Software**, II semestre 2026, Universidad de Costa Rica.

**Integrantes:** María Paula Leung Ching, Jimena Rivera Álvarez y Paula López Quesada.

## Alcance de esta iteración

Aplicación de consola en C# con dos casos de uso:

| Caso de uso | Historias | Operaciones |
|---|---|---|
| **CU-1:** Registrar y consultar productos | US-01, US-07, US-14 | Inserción y consulta |
| **CU-2:** Completar una venta en efectivo | US-16, US-19, US-21, US-22, US-24, US-25 | Transacción (venta + inventario, todo o nada) |

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)

## Cómo ejecutar

```bash
dotnet restore PosCafeteria.slnx
dotnet run --project src/Pos.ConsoleApp
```

La base de datos SQLite se crea automáticamente (`pos.db`) con datos de ejemplo.
Para usar otra ruta, defina la variable de entorno `POS_DB_PATH`.

## Pruebas y cobertura

```bash
dotnet test PosCafeteria.slnx --settings coverlet.runsettings --collect:"XPlat Code Coverage"
```

Para ver el reporte en HTML:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:coverage-report -reporttypes:Html
```

## Arquitectura

Arquitectura en capas (RNF-12, RNF-13). Las dependencias apuntan solo hacia el dominio:

```
Pos.ConsoleApp  ──►  Pos.Application  ──►  Pos.Domain
       │                    ▲
       └──►  Pos.Infrastructure (SQLite, implementa las interfaces de Application)
```

| Proyecto | Responsabilidad |
|---|---|
| `Pos.Domain` | Entidades y reglas de negocio. No depende de nada. |
| `Pos.Application` | Casos de uso e interfaces de repositorios. |
| `Pos.Infrastructure` | Acceso a la base de datos SQLite. |
| `Pos.ConsoleApp` | Menú de consola y conexión entre capas. |
| `Pos.Tests` | Pruebas unitarias y de integración (xUnit). |

## Flujo de trabajo (GitHub Flow)

- `main` siempre estable; no se hacen commits directos.
- Ramas: `feature/`, `fix/`, `refactor/`, `test/`, `docs/` + referencia breve (ej. `feature/cu1-registrar-producto`).
- Todo cambio entra por **pull request** con al menos una revisión aprobada y el CI en verde.
- Los defectos se registran como *issues* con la plantilla **Defecto**.

## Calidad

- **Análisis estático:** analizadores Roslyn del SDK de .NET (en cada compilación) y SonarQube Cloud.
- **Convenciones:** definidas en `.editorconfig` y aplicadas al compilar.
- **CI:** GitHub Actions compila, ejecuta las pruebas y publica el reporte de cobertura en cada PR.
