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

    public int Agregar(DispositivoRed dispositivo)
    {
        using var connection = _factory.CrearConexionEscritura();
        connection.Open();
        connection.Execute(
            "INSERT INTO dispositivos(nombre,tipo,latenciaMs) VALUES (@Nombre,@Tipo,@latenciaMs)",
            new
            {
                dispositivo.Nombre,
                Tipo = dispositivo.GetType().Name,
                dispositivo.latenciaMs
            });
        return connection.QuerySingle<int>("SELECT LAST_INSERT_ID()");
    }

    public bool Actualizar(DispositivoRed dispositivo)
    {
        using var connection = _factory.CrearConexionEscritura();
        var existe = connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM dispositivos WHERE id = @Id)", new { dispositivo.Id });
        if (!existe)
            return false;

        connection.Execute(
            "UPDATE dispositivos SET nombre = @Nombre, tipo = @Tipo, latenciaMs = @LatenciaMs WHERE id = @Id",
            new
            {
                dispositivo.Id,
                dispositivo.Nombre,
                Tipo = dispositivo.GetType().Name,
                LatenciaMs = dispositivo.latenciaMs
            });
        return true;
    }

    public ResultadoEliminacionDispositivo Eliminar(int id)
    {
        using var connection = _factory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var existe = connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM dispositivos WHERE id = @id)", new { id }, transaction);
        if (!existe)
            return ResultadoEliminacionDispositivo.NoExiste;

        var enUso = connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM recorridos WHERE dispositivo_id = @id)", new { id }, transaction);
        if (enUso)
            return ResultadoEliminacionDispositivo.EnUso;

        connection.Execute("DELETE FROM dispositivos WHERE id = @id", new { id }, transaction);
        transaction.Commit();
        return ResultadoEliminacionDispositivo.Eliminado;
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
            var tipoNormalizado when string.Equals(tipoNormalizado, nameof(Router), StringComparison.OrdinalIgnoreCase) =>
                new Router(id, nombre, latenciaMs),

            var tipoNormalizado when string.Equals(tipoNormalizado, nameof(Switch), StringComparison.OrdinalIgnoreCase) =>
                new Switch(id, nombre, latenciaMs),

            var tipoNormalizado when string.Equals(tipoNormalizado, nameof(AccessPoint), StringComparison.OrdinalIgnoreCase) =>
                new AccessPoint(id, nombre, latenciaMs),

            var tipoNormalizado when string.Equals(tipoNormalizado, nameof(Firewall), StringComparison.OrdinalIgnoreCase) =>
                new Firewall(id, nombre, latenciaMs),

            _ => throw new InvalidOperationException(
                $"Tipo de dispositivo desconocido: {tipo}")
        };
    }
}
