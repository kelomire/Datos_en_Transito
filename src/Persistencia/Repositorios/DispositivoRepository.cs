using Persistencia.Entidades;
using Persistencia.Interfaces;
using Dapper;

namespace Persistencia.Repositorios;

public class DispositivoRepository : IDispositivoRepository
{
    private readonly IDbConnectionFactory _factory;

    public DispositivoRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Agregar(DispositivoRed dispositivo)
    {
        using var connection = _factory.CrearConexionDesarrollo();

        connection.Execute(
            "INSERT INTO dispositivos(nombre,tipo,latencia) VALUES (@Nombre,@Tipo,@Latencia)",
            new
            {
                dispositivo.Nombre,
                Tipo = dispositivo.GetType().Name,
                dispositivo.Latencia
            });
    }

    public IEnumerable<DispositivoRed> ObtenerTodos()
    {
        using var connection = _factory.CrearConexionDesarrollo();

        var items = connection.Query<dynamic>(
            "SELECT id,nombre,tipo,latencia FROM dispositivos");

        return items.Select(x =>
            CrearDispositivo(
                (int)x.id,
                (string)x.nombre,
                (string)x.tipo,
                (int)x.latencia));
    }

    public DispositivoRed ObtenerPorId(int id)
    {
        using var connection = _factory.CrearConexionDesarrollo();

        var item = connection.QueryFirstOrDefault<dynamic>(
            "SELECT id,nombre,tipo,latencia FROM dispositivos WHERE id=@id",
            new { id });

        if (item == null)
            return null!;

        return CrearDispositivo(
            (int)item.id,
            (string)item.nombre,
            (string)item.tipo,
            (int)item.latencia);
    }

    private static DispositivoRed CrearDispositivo(
        int id,
        string nombre,
        string tipo,
        int latencia)
    {
        return tipo switch
        {
            nameof(Router) =>
                new Router(id, nombre, latencia),

            nameof(Switch) =>
                new Switch(id, nombre, latencia),

            nameof(AccessPoint) =>
                new AccessPoint(id, nombre, latencia),

            nameof(Firewall) =>
                new Firewall(id, nombre, latencia),

            _ => throw new InvalidOperationException(
                $"Tipo de dispositivo desconocido: {tipo}")
        };
    }
}
