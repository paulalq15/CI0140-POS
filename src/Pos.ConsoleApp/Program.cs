using Pos.ConsoleApp;
using Pos.Infrastructure.Persistence;

// Composición de dependencias: aquí se conectan las capas.
var connectionFactory = SqliteConnectionFactory.DesdeVariableDeEntorno();
new DatabaseInitializer(connectionFactory).Inicializar();

var menu = new MenuPrincipal();
menu.Ejecutar();
