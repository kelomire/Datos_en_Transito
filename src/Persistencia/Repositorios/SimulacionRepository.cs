using System.Data;
using Dapper;
using Datos_en_Transito.Persistencia;
using Persistencia.Entidades;
using Persistencia.Interfaces;

namespace Persistencia.Repositorios;

public class SimulacionRepository : ISimulacionRepository
{
    private readonly IDbConnectionFactory _factory;

    public SimulacionRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public int Registrar(Simulacion simulacion)
    {
        using var connection = _factory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        connection.Execute(
            "INSERT INTO paquetes(ip_origen, ip_destino, tamano_bytes, ttl_inicial) VALUES (@Origen, @Destino, @TamanioBytes, @Ttl)",
            new
            {
                Origen = simulacion.Paquete.IpOrigen,
                Destino = simulacion.Paquete.IpDestino,
                simulacion.Paquete.TamanoBytes,
                Ttl = simulacion.Paquete.TtlInicial
            }, transaction);
        var paqueteId = connection.QuerySingle<int>("SELECT LAST_INSERT_ID()", transaction: transaction);
        var simulacionId = InsertarCabecera(connection, transaction, paqueteId, simulacion);
        GuardarRecorrido(connection, transaction, simulacionId, simulacion);

        transaction.Commit();
        return simulacionId;
    }

    public bool Actualizar(int id, Simulacion simulacion)
    {
        using var connection = _factory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var paqueteId = connection.QuerySingleOrDefault<int?>(
            "SELECT paquete_id FROM simulaciones WHERE id = @id FOR UPDATE", new { id }, transaction);
        if (paqueteId is null)
            return false;

        connection.Execute(
            "UPDATE paquetes SET ip_origen = @Origen, ip_destino = @Destino, tamano_bytes = @TamanioBytes, ttl_inicial = @Ttl WHERE id = @PaqueteId",
            new
            {
                Origen = simulacion.Paquete.IpOrigen,
                Destino = simulacion.Paquete.IpDestino,
                simulacion.Paquete.TamanoBytes,
                Ttl = simulacion.Paquete.TtlInicial,
                PaqueteId = paqueteId.Value
            }, transaction);

        connection.Execute(
            "UPDATE simulaciones SET latenciaMs_total = @LatenciaTotalMs, ttl_final = @TtlFinal, entregado = @Entregado, descartado = @Descartado WHERE id = @id",
            new
            {
                id,
                LatenciaTotalMs = simulacion.LatenciaMsTotal,
                TtlFinal = simulacion.Paquete.Ttl,
                Entregado = simulacion.Paquete.Entregado,
                Descartado = simulacion.Paquete.Descartado
            }, transaction);

        connection.Execute("DELETE FROM recorridos WHERE simulacion_id = @id", new { id }, transaction);
        GuardarRecorrido(connection, transaction, id, simulacion);
        transaction.Commit();
        return true;
    }

    public SimulacionPersistida? ObtenerPorId(int id)
    {
        using var connection = _factory.CrearConexionLectura();
        var simulacion = connection.QuerySingleOrDefault<SimulacionPersistida>(
            """
            SELECT s.id AS Id, p.ip_origen AS Origen, p.ip_destino AS Destino,
                   p.tamano_bytes AS TamanioBytes, p.ttl_inicial AS Ttl,
                   s.ttl_final AS TtlFinal, s.latenciaMs_total AS LatenciaTotalMs,
                   s.entregado AS Entregado, s.descartado AS Descartado
            FROM simulaciones s
            INNER JOIN paquetes p ON p.id = s.paquete_id
            WHERE s.id = @id
            """, new { id });
        if (simulacion is null)
            return null;

        simulacion.Recorrido = ObtenerRecorrido(connection, id);
        return simulacion;
    }

    public IEnumerable<SimulacionPersistida> ObtenerTodas()
    {
        using var connection = _factory.CrearConexionLectura();
        return connection.Query<SimulacionPersistida>(
            """
            SELECT s.id AS Id, p.ip_origen AS Origen, p.ip_destino AS Destino,
                   p.tamano_bytes AS TamanioBytes, p.ttl_inicial AS Ttl,
                   s.ttl_final AS TtlFinal, s.latenciaMs_total AS LatenciaTotalMs,
                   s.entregado AS Entregado, s.descartado AS Descartado
            FROM simulaciones s
            INNER JOIN paquetes p ON p.id = s.paquete_id
            ORDER BY s.id
            """).AsList();
    }

    public bool Eliminar(int id)
    {
        using var connection = _factory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var paqueteId = connection.QuerySingleOrDefault<int?>(
            "SELECT paquete_id FROM simulaciones WHERE id = @id FOR UPDATE", new { id }, transaction);
        if (paqueteId is null)
            return false;

        connection.Execute("DELETE FROM recorridos WHERE simulacion_id = @id", new { id }, transaction);
        connection.Execute("DELETE FROM simulaciones WHERE id = @id", new { id }, transaction);
        connection.Execute("DELETE FROM paquetes WHERE id = @paqueteId", new { paqueteId }, transaction);
        transaction.Commit();
        return true;
    }

    private static int InsertarCabecera(
        IDbConnection connection,
        IDbTransaction transaction,
        int paqueteId,
        Simulacion simulacion)
    {
        connection.Execute(
            "INSERT INTO simulaciones(paquete_id, latenciaMs_total, ttl_final, entregado, descartado) VALUES (@PaqueteId, @LatenciaTotalMs, @TtlFinal, @Entregado, @Descartado)",
            new
            {
                PaqueteId = paqueteId,
                LatenciaTotalMs = simulacion.LatenciaMsTotal,
                TtlFinal = simulacion.Paquete.Ttl,
                Entregado = simulacion.Paquete.Entregado,
                Descartado = simulacion.Paquete.Descartado
            }, transaction);
        return connection.QuerySingle<int>("SELECT LAST_INSERT_ID()", transaction: transaction);
    }

    private static void GuardarRecorrido(
        IDbConnection connection,
        IDbTransaction transaction,
        int simulacionId,
        Simulacion simulacion)
    {
        for (var index = 0; index < simulacion.Resultados.Count; index++)
        {
            var resultado = simulacion.Resultados[index];
            var esUltimoPaso = index == simulacion.Resultados.Count - 1;
            connection.Execute(
                "INSERT INTO recorridos(simulacion_id, dispositivo_id, latenciaMs, ttl_final, entregado, descartado) VALUES (@SimulacionId, @DispositivoId, @LatenciaMs, @TtlFinal, @Entregado, @Descartado)",
                new
                {
                    SimulacionId = simulacionId,
                    resultado.DispositivoId,
                    resultado.LatenciaMs,
                    resultado.TtlFinal,
                    Entregado = esUltimoPaso && simulacion.Paquete.Entregado,
                    Descartado = esUltimoPaso && simulacion.Paquete.Descartado
                }, transaction);
        }
    }

    private static List<PasoRecorridoPersistido> ObtenerRecorrido(IDbConnection connection, int id)
        => connection.Query<PasoRecorridoPersistido>(
            """
            SELECT r.dispositivo_id AS DispositivoId, d.nombre AS Nombre, d.tipo AS Tipo,
                   r.latenciaMs AS LatenciaAplicadaMs, r.ttl_final AS TtlRestante
            FROM recorridos r
            INNER JOIN dispositivos d ON d.id = r.dispositivo_id
            WHERE r.simulacion_id = @id
            ORDER BY r.id
            """, new { id }).AsList();
}