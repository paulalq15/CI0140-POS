## Descripción

<!-- ¿Qué cambia este PR y por qué? -->

**Historia(s) / requisito(s):** <!-- Ej.: US-01, RF-01 -->

## Cómo probarlo

<!-- Pasos o comandos para comprobar el cambio. Ej.: dotnet test -->

---

## Lista de chequeo de revisión de pares (Etapa 2, sección 3.1.3)

**Llena el autor:**
- [ ] El código compila y todas las pruebas pasan localmente.
- [ ] Agregué o actualicé las pruebas de la funcionalidad.
- [ ] No hay advertencias nuevas de los analizadores (Roslyn / SonarQube).
- [ ] No incluí credenciales, secretos ni rutas personales (RNF-04).

**Llena quien revisa** (marca cada punto y deja un comentario si algo no se cumple):
- [ ] Los nombres de clases, métodos y variables siguen la convención de C# (PascalCase, camelCase, prefijo `I` en interfaces).
- [ ] Cada clase y método tiene una sola responsabilidad clara.
- [ ] No hay código duplicado.
- [ ] Se validan los datos de entrada antes de procesarlos (RNF-03).
- [ ] Los errores se manejan sin mostrar detalles internos del sistema (RNF-22).
- [ ] Los mensajes de error están en español, dicen qué falló e indican qué corregir (Métrica 6.2).
- [ ] Se respeta la separación de capas: Domain no depende de nada; Application solo de Domain; la consola no tiene lógica de negocio (RNF-12, RNF-13).
- [ ] Las pruebas cubren casos válidos, inválidos y de límite.
- [ ] El pipeline de CI terminó en verde.

**Resultado de la revisión:** <!-- Aprobado / Aprobado con observaciones / Cambios solicitados -->
**Defectos encontrados en la revisión:** <!-- Número y enlace a los issues, si los hay (Métrica 1.3) -->