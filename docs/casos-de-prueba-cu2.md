# Casos de prueba — CU-2: Completar una venta en efectivo

**Historias:** US-16 (crear una orden), US-19 (calcular el total de una orden), US-21 (verificar disponibilidad de inventario), US-22 (registrar pagos en efectivo), US-24 (completar y registrar una venta), US-25 (actualizar automáticamente el inventario después de una venta)

**Requisitos:** RNF-03, RNF-08, RNF-09, RNF-17, RNF-24

## Especificación

Datos iniciales de la base de datos: sucursal 1 con cajas 1 y 2; Agua 600 ml ₡800 (50 unidades), Té frío ₡1200 (30), Croissant ₡1500 (10) y Brownie ₡1300 (5).

| ID | Escenario | Datos de entrada | Resultado esperado | Prueba automatizada |
|---|---|---|---|---|
| CP-08 | Venta exitosa | Sucursal 1, caja 1, 2 × Agua + 1 × Croissant, efectivo ₡5000 | Total ₡3100, vuelto ₡1900; se guardan la venta y sus 2 detalles y se descuenta el inventario | `CompletarVentaCasoDeUsoTests.Ejecutar_VentaValida_CalculaTotalYVueltoYRegistraLaVenta`, `SqliteVentaRepositoryTests.RegistrarVenta_VentaValida_GuardaVentaYDetallesYDescuentaInventario`, `CompletarVentaIntegracionTests.Ejecutar_VentaValida_GuardaLaVentaYDescuentaElInventario`, `CompletarVentaIntegracionTests.Ejecutar_VentaValida_GuardaImportesYDetallesEnLaBaseDeDatos` |
| CP-09 | Efectivo exacto y precios con céntimos (valor límite) | 2 × Croissant con ₡3000; 3 × Galleta de ₡800,25 con ₡2400,75 o ₡2500 | Vuelto ₡0 con el monto exacto; con céntimos, total ₡2400,75 y vuelto ₡0 o ₡99,25, sin redondeos | `CompletarVentaCasoDeUsoTests.Ejecutar_MontoExacto_VueltoCero`, `CompletarVentaCasoDeUsoTests.Ejecutar_PreciosConCentimos_CalculaTotalYVueltoExactos` |
| CP-10 | Se vende todo el inventario (valor límite) | 10 × Croissant (hay 10) | La venta se permite | `CompletarVentaCasoDeUsoTests.Ejecutar_CantidadIgualAlInventario_SePermite` |
| CP-11 | Venta vacía | Sin productos | Se rechaza y no se guarda nada | `CompletarVentaCasoDeUsoTests.Ejecutar_SinProductos_LanzaValidacionYNoRegistra` |
| CP-12 | Cantidad inválida (valor límite) | Cantidad 0 o -1 | Se rechaza y no se guarda nada | `CompletarVentaCasoDeUsoTests.Ejecutar_CantidadCeroONegativa_LanzaValidacionYNoRegistra` |
| CP-13 | Producto inexistente o inactivo | Código 99, o un producto inactivo | Se rechaza con el código o el nombre del producto | `CompletarVentaCasoDeUsoTests.Ejecutar_ProductoInexistente_LanzaValidacionConElCodigo`, `CompletarVentaCasoDeUsoTests.Ejecutar_ProductoInactivo_LanzaValidacionYNoRegistra` |
| CP-14 | Inventario insuficiente | 11 × Croissant (hay 10); 1 × Croissant + 6 × Brownie (hay 5); o dos líneas que juntas pasan el máximo de `int` | Se rechaza indicando cuántas unidades hay; no se guarda la venta y el inventario queda igual | `CompletarVentaCasoDeUsoTests.Ejecutar_InventarioInsuficiente_LanzaValidacionConLaCantidadDisponible`, `CompletarVentaCasoDeUsoTests.Ejecutar_CantidadesQueJuntasSuperanElMaximoDeInt_RechazaPorInventario`, `SqliteVentaRepositoryTests.RegistrarVenta_SegundoProductoSinInventarioSuficiente_NoGuardaNadaYRevierteElInventario`, `CompletarVentaIntegracionTests.Ejecutar_InventarioInsuficiente_NoGuardaLaVentaYElInventarioQuedaIgual` |
| CP-15 | Efectivo insuficiente (valor límite) | Total ₡3100, efectivo ₡3099 | Se rechaza; no se guarda la venta y el inventario queda igual | `CompletarVentaCasoDeUsoTests.Ejecutar_MontoInsuficiente_LanzaValidacionYNoRegistra`, `CompletarVentaIntegracionTests.Ejecutar_MontoInsuficiente_NoGuardaLaVentaYElInventarioQuedaIgual` |
| CP-16 | Sucursal o caja inválidas | Sucursal 0 o 99; caja 0 o de otra sucursal; caja que pasa a otra sucursal antes de guardar | Se rechaza, no se guarda nada y el inventario queda igual | `CompletarVentaCasoDeUsoTests.Ejecutar_Sucursal*`, `CompletarVentaCasoDeUsoTests.Ejecutar_Caja*`, `SqliteVentaRepositoryTests.RegistrarVenta_CajaDeOtraSucursal_LanzaValidacionYNoGuarda`, `SqliteVentaRepositoryTests.RegistrarVenta_CajaYaNoPerteneceALaSucursal_NoGuardaLaVenta`, `CompletarVentaIntegracionTests.Ejecutar_CajaDeOtraSucursal_NoGuardaLaVentaYElInventarioQuedaIgual` |
| CP-17 | Ventas simultáneas (Métrica 2.1) | 10 ventas al mismo tiempo de 1 Brownie (hay 5) | Exactamente 5 ventas se guardan, 5 se rechazan y el inventario queda en 0, nunca negativo | `SqliteVentaRepositoryTests.RegistrarVenta_VentasSimultaneasDelMismoProducto_NoVendeMasDeLoQueHay` |

## Casos de error y sus mensajes (Métricas 6.1 y 6.2)

| Caso de error | Mensaje |
|---|---|
| Sin sucursal | La sucursal es obligatoria. Digite el número de una sucursal válida. |
| Sucursal inexistente | La sucursal indicada no existe. Digite el número de una sucursal válida. |
| Sin caja | La caja es obligatoria. Digite el número de una caja de la sucursal. |
| Caja de otra sucursal | La caja indicada no existe en esta sucursal. Digite el número de una caja de la sucursal seleccionada. |
| Efectivo negativo | El efectivo recibido no puede ser negativo. Digite el monto recibido. |
| Venta vacía | La venta está vacía. Agregue al menos un producto. |
| Código de producto inválido | El código de producto es inválido. Digite un código del catálogo. |
| Cantidad cero o negativa | La cantidad debe ser mayor que cero. Digite al menos una unidad. |
| Producto inexistente | No existe el producto con código {código}. Revise el código en el catálogo. |
| Producto inactivo | El producto '{nombre}' está inactivo. Elija un producto activo del catálogo. |
| Inventario insuficiente | Hay {n} unidades de '{nombre}' en la sucursal. Reduzca la cantidad o seleccione otro producto. |
| Efectivo insuficiente | El efectivo recibido no cubre el total. Digite un monto igual o superior al total de la venta. |
| Total demasiado alto | El total de la venta es demasiado alto. Reduzca las cantidades o divida la compra. |
| La caja deja de ser válida al guardar | No se pudo registrar la venta en la caja indicada. Vuelva a seleccionar una caja de la sucursal. |
| Otra caja vendió el inventario al mismo tiempo | Ya no hay suficiente inventario del producto {código}. Consulte el inventario y ajuste la cantidad. |

Los 15 casos de error tienen un mensaje propio (Métrica 6.1 = 100 %).

## Tipos de prueba

- **Unitarias (sin base de datos, RNF-17):** `Pos.Tests/Application/CompletarVentaCasoDeUsoTests.cs`, con repositorios falsos en memoria.
- **Integración (SQLite real en un archivo temporal):** `Pos.Tests/Infrastructure/SqliteVentaRepositoryTests.cs` (transacción, reversión, ventas simultáneas y cajas) y `Pos.Tests/Infrastructure/CompletarVentaIntegracionTests.cs` (caso de uso completo con la base de datos).
