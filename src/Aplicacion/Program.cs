using Aplicacion.Entidades;

Console.WriteLine("=== DATOS EN TRÁNSITO ===");
Console.WriteLine();

Console.Write("IP de origen: ");
string ipOrigen = Console.ReadLine() ?? "";

Console.Write("IP de destino: ");
string ipDestino = Console.ReadLine() ?? "";

Console.Write("Tamaño del paquete (bytes): ");
int tamano = int.Parse(Console.ReadLine() ?? "0");

Console.Write("TTL inicial: ");
int ttl = int.Parse(Console.ReadLine() ?? "0");

var paquete = new PaqueteRed(
    ipOrigen,
    ipDestino,
    tamano,
    ttl
);

Console.WriteLine();
Console.WriteLine("Seleccione los dispositivos del recorrido.");
Console.WriteLine("1 - Router");
Console.WriteLine("2 - Switch");
Console.WriteLine("3 - Access Point");
Console.WriteLine("4 - Firewall");
Console.WriteLine("5 - Terminar recorrido");
Console.WriteLine();

var dispositivos = new List<DispositivoRed>();
int id = 1;

while (true)
{
    Console.Write("Seleccione dispositivo: ");
    string opcion = Console.ReadLine() ?? "";

    switch (opcion)
    {
        case "1":
            dispositivos.Add(new Router(id++, $"Router {id - 1}"));
            break;

        case "2":
            dispositivos.Add(new Switch(id++, $"Switch {id - 1}"));
            break;

        case "3":
            dispositivos.Add(new AccessPoint(id++, $"Access Point {id - 1}"));
            break;

        case "4":
            dispositivos.Add(new Firewall(id++, $"Firewall {id - 1}"));
            break;

        case "5":
            if (dispositivos.Count == 0)
            {
                Console.WriteLine("Debe agregar al menos un dispositivo.");
                continue;
            }

            break;

        default:
            Console.WriteLine("Opción inválida.");
            continue;
    }

    if (opcion == "5")
        break;

    Console.WriteLine("Dispositivo agregado.");
}

var simulacion = new Simulacion(paquete, dispositivos);

simulacion.Ejecutar();

Console.WriteLine();
Console.WriteLine("=== RESULTADO ===");
Console.WriteLine($"IP origen:  {paquete.IpOrigen}");
Console.WriteLine($"IP destino: {paquete.IpDestino}");
Console.WriteLine($"Tamaño:     {paquete.TamanoBytes} bytes");
Console.WriteLine($"TTL final:  {paquete.Ttl}");
Console.WriteLine($"Entregado:  {paquete.Entregado}");
Console.WriteLine($"Descartado: {paquete.Descartado}");

Console.WriteLine();
Console.WriteLine("=== RECORRIDO ===");

foreach (var resultado in simulacion.Resultados)
{
    Console.WriteLine(
        $"{resultado.TipoDispositivo} | " +
        $"Latencia: {resultado.Latencia} ms | " +
        $"TTL: {resultado.TtlFinal} | " +
        $"Descartado: {resultado.Descartado}"
    );
}
