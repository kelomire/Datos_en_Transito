using Persistencia.Entidades;
using Persistencia.Interfaces;
using Dapper;

namespace Persistencia.Repositorios;

public class SimulacionRepository : ISimulacionRepository
{
    private readonly IDbConnectionFactory _factory;

    public SimulacionRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Registrar(Simulacion simulacion)
    {
        using var connection = _factory.CrearConexionDesarrollo();

        connection.Execute(
            "sp_registrar_simulacion",
            new
            {
                IpOrigen = simulacion.Paquete.IpOrigen,
                IpDestino = simulacion.Paquete.IpDestino,
                Tamano = simulacion.Paquete.TamanoBytes,
                Ttl = simulacion.Paquete.Ttl,
                Latencia = simulacion.LatenciaTotal,
                Entregado = simulacion.Paquete.Entregado,
                Descartado = simulacion.Paquete.Descartado
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public Simulacion? ObtenerPorId(int id)
    {
        using var connection = _factory.CrearConexionDesarrollo();

        var data = connection.QueryFirstOrDefault<dynamic>(
            """
            SELECT
                s.id,
                s.latencia_total,
                s.ttl_final,
                s.entregado,
                s.descartado,
                p.ip_origen,
                p.ip_destino,
                p.tamano_bytes,
                p.ttl_inicial
            FROM simulaciones s
            INNER JOIN paquetes p ON p.id = s.paquete_id
            WHERE s.id = @id
            """,
            new { id });

        if (data == null)
            return null;

        var paquete = new PaqueteRed(
            (string)data.ip_origen,
            (string)data.ip_destino,
            (int)data.tamano_bytes,
            (int)data.ttl_inicial
        );

        paquete.Entregado = (bool)data.entregado;
        paquete.Descartado = (bool)data.descartado;

        return new Simulacion(
            paquete,
            new List<DispositivoRed>()
        );
    }

    public IEnumerable<Simulacion> ObtenerTodas()
    {
        using var connection = _factory.CrearConexionDesarrollo();

        var data = connection.Query<dynamic>(
            """
            SELECT
                s.id,
                s.latencia_total,
                s.ttl_final,
                s.entregado,
                s.descartado,
                p.ip_origen,
                p.ip_destino,
                p.tamano_bytes,
                p.ttl_inicial
            FROM simulaciones s
            INNER JOIN paquetes p ON p.id = s.paquete_id
            ORDER BY s.id
            """);

        var simulaciones = new List<Simulacion>();

        foreach (var item in data)
        {
            var paquete = new PaqueteRed(
                (string)item.ip_origen,
                (string)item.ip_destino,
                (int)item.tamano_bytes,
                (int)item.ttl_inicial
            );

            paquete.Entregado = (bool)item.entregado;
            paquete.Descartado = (bool)item.descartado;

            simulaciones.Add(
                new Simulacion(
                    paquete,
                    new List<DispositivoRed>()
                )
            );
        }

        return simulaciones;
    }
}
