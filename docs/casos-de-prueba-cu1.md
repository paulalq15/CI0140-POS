# Casos de prueba — CU-1: Registrar y consultar productos

**Historias:** US-01 (registrar productos), US-07 (consultar el catálogo), US-14 (consultar inventario por sucursal)
**Requisitos:** RF-01, RF-05, RF-13, RNF-03, RNF-17, RNF-24

## Especificación

| ID | Operación | Escenario | Datos de entrada | Resultado esperado | Prueba automatizada |
|---|---|---|---|---|---|
| CP-01 | Inserción | Registro exitoso | Nombre "Galleta de avena", precio ₡900, categoría Repostería (activa) | Se guarda el producto activo, con el nombre sin espacios sobrantes, y se devuelve su código | `RegistrarProductoCasoDeUsoTests.Ejecutar_DatosValidos_RegistraProductoActivoYDevuelveId`, `SqliteProductoRepositoryTests.Insertar_ProductoValido_SePuedeObtenerConLosMismosDatos` |
| CP-02 | Inserción | Precio inválido (valor límite) | Precio ₡0, -₡1 o -₡1500,50 | Se rechaza con el mensaje "El precio debe ser mayor que cero. Digite un precio válido." y no se guarda nada | `RegistrarProductoCasoDeUsoTests.Ejecutar_PrecioCero_LanzaValidacionYNoGuarda`, `ProductoTests.Validar_PrecioCeroONegativo_LanzaValidacion` |
| CP-03 | Inserción | Nombre repetido | "BROWNIE" cuando ya existe "Brownie" | Se rechaza con "Ya existe un producto con el nombre…" y no se duplica | `RegistrarProductoCasoDeUsoTests.Ejecutar_NombreRepetido_LanzaValidacionYNoDuplica`, `SqliteProductoRepositoryTests.ExisteNombre_NombreRegistrado_DevuelveTrueSinImportarMayusculas` |
| CP-04 | Consulta | Catálogo organizado por categoría | Productos activos en Bebidas y Repostería | Grupos por categoría en orden alfabético, con los productos ordenados por nombre | `ConsultarCatalogoCasoDeUsoTests.Ejecutar_ProductosActivos_AgrupaPorCategoriaYOrdenaPorNombre` |
| CP-05 | Consulta | Productos inactivos y categorías deshabilitadas | Un producto inactivo y una categoría deshabilitada | No aparecen en el catálogo | `ConsultarCatalogoCasoDeUsoTests.Ejecutar_ProductoInactivoYCategoriaDeshabilitada_NoSeMuestran` |
| CP-06 | Consulta | Inventario de una sucursal existente | Sucursal 1 | Lista de productos activos con su cantidad; los que no tienen registro aparecen con 0 | `ConsultarInventarioCasoDeUsoTests.Ejecutar_SucursalExistente_DevuelveSoloSuInventario`, `SqliteInventarioRepositoryTests.ObtenerPorSucursal_*` |
| CP-07 | Consulta | Sucursal inexistente | Sucursal 0 o 99 | Se rechaza con "La sucursal indicada no existe. Elija una sucursal de la lista." | `ConsultarInventarioCasoDeUsoTests.Ejecutar_SucursalInexistente_LanzaValidacionConMensajeEspecifico` |

## Casos de error y sus mensajes (Métricas 6.1 y 6.2)

| Caso de error | Mensaje |
|---|---|
| Nombre vacío | El nombre del producto es obligatorio. Escriba un nombre. |
| Nombre de más de 100 caracteres | El nombre del producto no puede tener más de 100 caracteres. Escriba un nombre más corto. |
| Descripción de más de 250 caracteres | La descripción no puede tener más de 250 caracteres. Escriba una descripción más corta. |
| Precio cero o negativo | El precio debe ser mayor que cero. Digite un precio válido. |
| Sin categoría | Debe seleccionar una categoría. Elija una categoría de la lista. |
| Categoría inexistente | La categoría seleccionada no existe. Elija una categoría de la lista. |
| Categoría deshabilitada | La categoría seleccionada está deshabilitada. Elija una categoría activa. |
| Nombre repetido | Ya existe un producto con el nombre "…". Use un nombre diferente. |
| Sucursal inexistente | La sucursal indicada no existe. Elija una sucursal de la lista. |

Los 9 casos de error tienen un mensaje propio (Métrica 6.1 = 100 %).

## Tipos de prueba

- **Unitarias (sin base de datos, RNF-17):** `Pos.Tests/Domain` y `Pos.Tests/Application`, con repositorios falsos en memoria.
- **Integración (SQLite real en un archivo temporal):** `Pos.Tests/Infrastructure`.
