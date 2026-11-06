using System.Data;
using API.Models;
using Dapper;
using Datos_en_Transito.Persistencia;
using Persistencia.Entidades;

namespace API.Servicios;

public sealed class ApiDatabaseService
{
    private static readonly string[] TiposValidos = ["Router", "Switch", "Firewall", "AccessPoint"];
    private readonly IDbConnectionFactory _connectionFactory;

    public ApiDatabaseService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<Dispositivo> ListarDispositivos()
    {
        using var connection = _connectionFactory.CrearConexionLectura();
        return connection.Query<Dispositivo>(
            "SELECT id, nombre, tipo, latenciaMs FROM dispositivos ORDER BY id").AsList();
    }

    public Dispositivo? ObtenerDispositivo(int id)
    {
        using var connection = _connectionFactory.CrearConexionLectura();
        return connection.QuerySingleOrDefault<Dispositivo>(
            "SELECT id, nombre, tipo, latenciaMs FROM dispositivos WHERE id = @id", new { id });
    }

    public int CrearDispositivo(string nombre, string tipo, int latenciaMs)
    {
        using var connection = _connectionFactory.CrearConexionEscritura();
        connection.Open();
        connection.Execute(
            "INSERT INTO dispositivos(nombre, tipo, latenciaMs) VALUES (@nombre, @tipo, @latenciaMs)",
            new { nombre, tipo, latenciaMs });
        return connection.QuerySingle<int>("SELECT LAST_INSERT_ID()");
    }

    public bool ActualizarDispositivo(int id, string nombre, string tipo, int latenciaMs)
    {
        using var connection = _connectionFactory.CrearConexionEscritura();
        var existe = connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM dispositivos WHERE id = @id)", new { id });
        if (!existe)
            return false;

        connection.Execute(
            "UPDATE dispositivos SET nombre = @nombre, tipo = @tipo, latenciaMs = @latenciaMs WHERE id = @id",
            new { id, nombre, tipo, latenciaMs });
        return true;
    }

    public bool EliminarDispositivo(int id)
    {
        using var connection = _connectionFactory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var estaReferenciado = connection.ExecuteScalar<bool>(
            "SELECT EXISTS(SELECT 1 FROM recorridos WHERE dispositivo_id = @id)",
            new { id }, transaction);
        if (estaReferenciado)
            return false;

        var eliminado = connection.Execute(
            "DELETE FROM dispositivos WHERE id = @id", new { id }, transaction) > 0;
        transaction.Commit();
        return eliminado;
    }

    public SimulacionApi? EjecutarSimulacion(
        string origen,
        string destino,
        int tamanioBytes,
        int ttl,
        IReadOnlyList<int> dispositivoIds,
        int? id = null)
    {
        var dispositivos = ObtenerDispositivosParaSimular(dispositivoIds);
        if (dispositivos is null)
            return null;

        var simulacion = CalcularSimulacion(id ?? 0, origen, destino, tamanioBytes, ttl, dispositivos);
        using var connection = _connectionFactory.CrearConexionEscritura();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        if (id is null)
        {
            connection.Execute(
                "INSERT INTO paquetes(ip_origen, ip_destino, tamano_bytes, ttl_inicial) VALUES (@Origen, @Destino, @TamanioBytes, @Ttl)",
                new { simulacion.Origen, simulacion.Destino, simulacion.TamanioBytes, simulacion.Ttl }, transaction);
            var paqueteId = connection.QuerySingle<int>("SELECT LAST_INSERT_ID()", transaction: transaction);

            connection.Execute(
                "INSERT INTO simulaciones(paquete_id, latenciaMs_total, ttl_final, entregado, descartado) VALUES (@PaqueteId, @LatenciaTotalMs, @TtlFinal, @Entregado, @Descartado)",
                new
                {
                    PaqueteId = paqueteId,
                    simulacion.LatenciaTotalMs,
                    simulacion.TtlFinal,
                    Entregado = simulacion.Estado == "entregado",
                    Descartado = simulacion.Estado == "descartado"
                }, transaction);
            simulacion.Id = connection.QuerySingle<int>("SELECT LAST_INSERT_ID()", transaction: transaction);
        }
        else
        {
            var paqueteId = connection.QuerySingleOrDefault<int?>(
                "SELECT paquete_id FROM simulaciones WHERE id = @id FOR UPDATE", new { id }, transaction);
            if (paqueteId is null)
                return null;

            connection.Execute(
                "UPDATE paquetes SET ip_origen = @Origen, ip_destino = @Destino, tamano_bytes = @TamanioBytes, ttl_inicial = @Ttl WHERE id = @PaqueteId",
                new { simulacion.Origen, simulacion.Destino, simulacion.TamanioBytes, simulacion.Ttl, PaqueteId = paqueteId }, transaction);
            connection.Execute(
                "UPDATE simulaciones SET latenciaMs_total = @LatenciaTotalMs, ttl_final = @TtlFinal, entregado = @Entregado, descartado = @Descartado WHERE id = @id",
                new
                {
                    id,
                    simulacion.LatenciaTotalMs,
                    simulacion.TtlFinal,
                    Entregado = simulacion.Estado == "entregado",
                    Descartado = simulacion.Estado == "descartado"
                }, transaction);
            connection.Execute("DELETE FROM recorridos WHERE simulacion_id = @id", new { id }, transaction);
            simulacion.Id = id.Value;
        }

        GuardarRecorrido(connection, transaction, simulacion);
        transaction.Commit();
        return simulacion;
    }

    public SimulacionApi? ObtenerSimulacion(int id)
    {
        using var connection = _connectionFactory.CrearConexionLectura();
        var cabecera = connection.QuerySingleOrDefault<CabeceraSimulacion>(
            """
            SELECT s.id, p.ip_origen AS Origen, p.ip_destino AS Destino,
                   p.tamano_bytes AS TamanioBytes, p.ttl_inicial AS Ttl,
                   s.ttl_final AS TtlFinal, s.latenciaMs_total AS LatenciaTotalMs,
                   s.entregado AS Entregado, s.descartado AS Descartado
            FROM simulaciones s
            INNER JOIN paquetes p ON p.id = s.paquete_id
            WHERE s.id = @id
            """, new { id });
        if (cabecera is null)
            return null;

        var pasos = connection.Query<PasoSimulacion>(
            """
            SELECT r.dispositivo_id AS DispositivoId, d.nombre AS Nombre, d.tipo AS Tipo,
                   r.latenciaMs AS LatenciaAplicadaMs, r.ttl_final AS TtlRestante
            FROM recorridos r
            INNER JOIN dispositivos d ON d.id = r.dispositivo_id
            WHERE r.simulacion_id = @id
            ORDER BY r.id
            """, new { id }).AsList();

        return CrearResultado(cabecera, pasos);
    }

    public IReadOnlyList<ResumenSimulacion> ListarSimulaciones()
    {
        using var connection = _connectionFactory.CrearConexionLectura();
        return connection.Query<ResumenSimulacion>(
            "SELECT id, entregado AS Entregado, descartado AS Descartado, ttl_final AS TtlFinal, latenciaMs_total AS LatenciaTotalMs FROM simulaciones ORDER BY id")
            .AsList();
    }

    public bool EliminarSimulacion(int id)
    {
        using var connection = _connectionFactory.CrearConexionEscritura();
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

    private IReadOnlyList<Dispositivo>? ObtenerDispositivosParaSimular(IReadOnlyList<int> ids)
    {
        using var connection = _connectionFactory.CrearConexionLectura();
        var dispositivos = connection.Query<Dispositivo>(
            "SELECT id, nombre, tipo, latenciaMs FROM dispositivos WHERE id IN @ids", new { ids }).AsList();
        if (dispositivos.Count != ids.Distinct().Count())
            return null;

        var porId = dispositivos.ToDictionary(d => d.Id);
        return ids.Select(id => porId[id]).ToList();
    }

    private static SimulacionApi CalcularSimulacion(
        int id,
        string origen,
        string destino,
        int tamanioBytes,
        int ttl,
        IReadOnlyList<Dispositivo> dispositivos)
    {
        var dispositivosDominio = dispositivos.Select(dispositivo =>
        {
            var nombre = dispositivo.Nombre ?? string.Empty;
            var tipo = CanonicalizarTipo(dispositivo.Tipo ?? string.Empty);
            return tipo switch
            {
                nameof(Router) => (DispositivoRed)new Router(dispositivo.Id, nombre, dispositivo.LatenciaMs),
                nameof(Switch) => new Switch(dispositivo.Id, nombre, dispositivo.LatenciaMs),
                nameof(Firewall) => new Firewall(dispositivo.Id, nombre, dispositivo.LatenciaMs),
                nameof(AccessPoint) => new AccessPoint(dispositivo.Id, nombre, dispositivo.LatenciaMs),
                _ => throw new InvalidOperationException($"Tipo de dispositivo desconocido: {tipo}")
            };
        }).ToList();

        var paquete = new PaqueteRed(origen, destino, tamanioBytes, ttl);
        var simulacionDominio = new Persistencia.Entidades.Simulacion(paquete, dispositivosDominio);
        simulacionDominio.Ejecutar();

        var pasos = simulacionDominio.Resultados.Select(resultado => new PasoSimulacion
        {
            DispositivoId = resultado.DispositivoId,
            Nombre = dispositivos.First(d => d.Id == resultado.DispositivoId).Nombre ?? string.Empty,
            Tipo = resultado.TipoDispositivo,
            LatenciaAplicadaMs = resultado.latenciaMs,
            TtlRestante = resultado.TtlFinal
        }).ToList();

        return new SimulacionApi
        {
            Id = id,
            Origen = origen,
            Destino = destino,
            TamanioBytes = tamanioBytes,
            Ttl = ttl,
            Estado = paquete.Descartado ? "descartado" : "entregado",
            TtlFinal = paquete.Ttl,
            LatenciaTotalMs = simulacionDominio.latenciaMsTotal,
            Recorrido = pasos
        };
    }

    private static void GuardarRecorrido(IDbConnection connection, IDbTransaction transaction, SimulacionApi simulacion)
    {
        for (var index = 0; index < simulacion.Recorrido.Count; index++)
        {
            var paso = simulacion.Recorrido[index];
            connection.Execute(
                "INSERT INTO recorridos(simulacion_id, dispositivo_id, latenciaMs, ttl_final, entregado, descartado) VALUES (@SimulacionId, @DispositivoId, @Latencia, @Ttl, @Entregado, @Descartado)",
                new
                {
                    SimulacionId = simulacion.Id,
                    paso.DispositivoId,
                    Latencia = paso.LatenciaAplicadaMs,
                    Ttl = paso.TtlRestante,
                    Entregado = simulacion.Estado == "entregado" && index == simulacion.Recorrido.Count - 1,
                    Descartado = simulacion.Estado == "descartado" && index == simulacion.Recorrido.Count - 1
                }, transaction);
        }
    }

    private static SimulacionApi CrearResultado(CabeceraSimulacion cabecera, IReadOnlyList<PasoSimulacion> pasos)
    {
        for (var index = 0; index < pasos.Count; index++)
            pasos[index].Orden = index + 1;

        return new SimulacionApi
        {
            Id = cabecera.Id,
            Origen = cabecera.Origen,
            Destino = cabecera.Destino,
            TamanioBytes = cabecera.TamanioBytes,
            Ttl = cabecera.Ttl,
            Estado = cabecera.Descartado ? "descartado" : "entregado",
            TtlFinal = cabecera.TtlFinal,
            LatenciaTotalMs = cabecera.LatenciaTotalMs,
            Recorrido = pasos.ToList()
        };
    }

    private static string CanonicalizarTipo(string tipo) =>
        TiposValidos.FirstOrDefault(t => string.Equals(t, tipo, StringComparison.OrdinalIgnoreCase))
        ?? tipo;

    private sealed class CabeceraSimulacion
    {
        public int Id { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public int TamanioBytes { get; set; }
        public int Ttl { get; set; }
        public int TtlFinal { get; set; }
        public int LatenciaTotalMs { get; set; }
        public bool Entregado { get; set; }
        public bool Descartado { get; set; }
    }
}

public sealed class SimulacionApi
{
    public int Id { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public int TamanioBytes { get; set; }
    public int Ttl { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
    public List<PasoSimulacion> Recorrido { get; set; } = [];
}

public sealed class PasoSimulacion
{
    public int Orden { get; set; }
    public int DispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int LatenciaAplicadaMs { get; set; }
    public int TtlRestante { get; set; }
}

public sealed class ResumenSimulacion
{
    public int Id { get; set; }
    public bool Entregado { get; set; }
    public bool Descartado { get; set; }
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
    public string Estado => Descartado ? "descartado" : "entregado";
}
