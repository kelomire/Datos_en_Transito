using Persistencia.Entidades;
using Persistencia.Interfaces;
using Dapper;
using Datos_en_Transito.Persistencia;

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
        using var connection = _factory.CrearConexionEscritura();

        connection.Execute(
            "INSERT INTO dispositivos(nombre,tipo,latenciaMs) VALUES (@Nombre,@Tipo,@latenciaMs)",
            new
            {
                dispositivo.Nombre,
                Tipo = dispositivo.GetType().Name,
                dispositivo.latenciaMs
            });
    }

    public IEnumerable<DispositivoRed> ObtenerTodos()
    {
        using var connection = _factory.CrearConexionLectura();

        var items = connection.Query<dynamic>(
            "SELECT id,nombre,tipo,latenciaMs FROM dispositivos");

        return items.Select(x =>
            CrearDispositivo(
                (int)x.id,
                (string)x.nombre,
                (string)x.tipo,
                (int)x.latenciaMs));
    }

    public DispositivoRed ObtenerPorId(int id)
    {
        using var connection = _factory.CrearConexionLectura();

        var item = connection.QueryFirstOrDefault<dynamic>(
            "SELECT id,nombre,tipo,latenciaMs FROM dispositivos WHERE id=@id",
            new { id });

        if (item == null)
            return null!;

        return CrearDispositivo(
            (int)item.id,
            (string)item.nombre,
            (string)item.tipo,
            (int)item.latenciaMs);
    }

    private static DispositivoRed CrearDispositivo(
        int id,
        string nombre,
        string tipo,
        int latenciaMs)
    {
        return tipo switch
        {
            nameof(Router) =>
                new Router(id, nombre, latenciaMs),

            nameof(Switch) =>
                new Switch(id, nombre, latenciaMs),

            nameof(AccessPoint) =>
                new AccessPoint(id, nombre, latenciaMs),

            nameof(Firewall) =>
                new Firewall(id, nombre, latenciaMs),

            _ => throw new InvalidOperationException(
                $"Tipo de dispositivo desconocido: {tipo}")
        };
    }
}
