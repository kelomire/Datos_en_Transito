using Aplicacion.Modelos;
using Persistencia.Entidades;
using Persistencia.Interfaces;

namespace Aplicacion.Servicios;

public class SimulacionService
{
    private readonly ISimulacionRepository _simulacionRepository;
    private readonly IDispositivoRepository _dispositivoRepository;

    public SimulacionService(
        ISimulacionRepository simulacionRepository,
        IDispositivoRepository dispositivoRepository)
    {
        _simulacionRepository = simulacionRepository;
        _dispositivoRepository = dispositivoRepository;
    }

    public Simulacion Ejecutar(PaqueteRed paquete, IEnumerable<DispositivoRed> dispositivos)
    {
        var lista = dispositivos.ToList();
        if (lista.Count == 0)
            throw new ArgumentException("Debe existir al menos un dispositivo.", nameof(dispositivos));

        var simulacion = new Simulacion(paquete, lista);
        simulacion.Ejecutar();
        simulacion.Id = _simulacionRepository.Registrar(simulacion);
        return simulacion;
    }

    public SimulacionResultado? Ejecutar(
        string origen,
        string destino,
        int tamanioBytes,
        int ttl,
        IReadOnlyList<int> dispositivoIds)
    {
        var simulacion = CrearSimulacion(origen, destino, tamanioBytes, ttl, dispositivoIds);
        if (simulacion is null)
            return null;

        simulacion.Id = _simulacionRepository.Registrar(simulacion);
        return Convertir(simulacion);
    }

    public SimulacionResultado? Actualizar(
        int id,
        string origen,
        string destino,
        int tamanioBytes,
        int ttl,
        IReadOnlyList<int> dispositivoIds)
    {
        var simulacion = CrearSimulacion(origen, destino, tamanioBytes, ttl, dispositivoIds);
        if (simulacion is null || !_simulacionRepository.Actualizar(id, simulacion))
            return null;

        simulacion.Id = id;
        return Convertir(simulacion);
    }

    public SimulacionResultado? Obtener(int id)
    {
        var simulacion = _simulacionRepository.ObtenerPorId(id);
        return simulacion is null ? null : Convertir(simulacion);
    }

    public IEnumerable<ResumenSimulacionResultado> Listar()
        => _simulacionRepository.ObtenerTodas().Select(simulacion => new ResumenSimulacionResultado
        {
            Id = simulacion.Id,
            Estado = simulacion.Descartado ? "descartado" : "entregado",
            TtlFinal = simulacion.TtlFinal,
            LatenciaTotalMs = simulacion.LatenciaTotalMs
        });

    public bool Eliminar(int id)
        => _simulacionRepository.Eliminar(id);

    public SimulacionResultado? ActualizarPaso(int id, int orden, int dispositivoId)
    {
        var actual = _simulacionRepository.ObtenerPorId(id);
        if (actual is null)
            return null;
        if (orden <= 0 || orden > actual.Recorrido.Count)
            throw new ArgumentOutOfRangeException(nameof(orden), "El orden del paso indicado no existe.");
        if (_dispositivoRepository.ObtenerPorId(dispositivoId) is null)
            return null;

        var ids = actual.Recorrido.Select(paso => paso.DispositivoId).ToList();
        ids[orden - 1] = dispositivoId;
        return Actualizar(id, actual.Origen, actual.Destino, actual.TamanioBytes, actual.Ttl, ids);
    }

    public SimulacionResultado? EliminarPaso(int id, int orden)
    {
        var actual = _simulacionRepository.ObtenerPorId(id);
        if (actual is null)
            return null;
        if (orden <= 0 || orden > actual.Recorrido.Count)
            throw new ArgumentOutOfRangeException(nameof(orden), "El orden del paso indicado no existe.");
        if (actual.Recorrido.Count == 1)
            throw new InvalidOperationException("El recorrido debe conservar al menos un dispositivo.");

        var ids = actual.Recorrido.Select(paso => paso.DispositivoId).ToList();
        ids.RemoveAt(orden - 1);
        return Actualizar(id, actual.Origen, actual.Destino, actual.TamanioBytes, actual.Ttl, ids);
    }

    private Simulacion? CrearSimulacion(
        string origen,
        string destino,
        int tamanioBytes,
        int ttl,
        IReadOnlyList<int> dispositivoIds)
    {
        if (dispositivoIds.Count == 0)
            throw new ArgumentException("Debe indicar al menos un dispositivo en el recorrido.", nameof(dispositivoIds));

        var dispositivos = new List<DispositivoRed>(dispositivoIds.Count);
        foreach (var dispositivoId in dispositivoIds)
        {
            var dispositivo = _dispositivoRepository.ObtenerPorId(dispositivoId);
            if (dispositivo is null)
                return null;
            dispositivos.Add(dispositivo);
        }

        var simulacion = new Simulacion(new PaqueteRed(origen, destino, tamanioBytes, ttl), dispositivos);
        simulacion.Ejecutar();
        return simulacion;
    }

    private static SimulacionResultado Convertir(Simulacion simulacion)
    {
        var porId = simulacion.Dispositivos
            .GroupBy(dispositivo => dispositivo.Id)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.First());
        return new SimulacionResultado
        {
            Id = simulacion.Id,
            Origen = simulacion.Paquete.IpOrigen,
            Destino = simulacion.Paquete.IpDestino,
            TamanioBytes = simulacion.Paquete.TamanoBytes,
            Ttl = simulacion.Paquete.TtlInicial,
            Estado = simulacion.Paquete.Descartado ? "descartado" : "entregado",
            TtlFinal = simulacion.Paquete.Ttl,
            LatenciaTotalMs = simulacion.LatenciaMsTotal,
            Recorrido = simulacion.Resultados.Select((resultado, indice) => new PasoSimulacionResultado
            {
                Orden = indice + 1,
                DispositivoId = resultado.DispositivoId,
                Nombre = porId[resultado.DispositivoId].Nombre,
                Tipo = porId[resultado.DispositivoId].GetType().Name,
                LatenciaAplicadaMs = resultado.LatenciaMs,
                TtlRestante = resultado.TtlFinal
            }).ToList()
        };
    }

    private static SimulacionResultado Convertir(SimulacionPersistida simulacion)
        => new()
        {
            Id = simulacion.Id,
            Origen = simulacion.Origen,
            Destino = simulacion.Destino,
            TamanioBytes = simulacion.TamanioBytes,
            Ttl = simulacion.Ttl,
            Estado = simulacion.Descartado ? "descartado" : "entregado",
            TtlFinal = simulacion.TtlFinal,
            LatenciaTotalMs = simulacion.LatenciaTotalMs,
            Recorrido = simulacion.Recorrido.Select((paso, indice) => new PasoSimulacionResultado
            {
                Orden = indice + 1,
                DispositivoId = paso.DispositivoId,
                Nombre = paso.Nombre,
                Tipo = paso.Tipo,
                LatenciaAplicadaMs = paso.LatenciaAplicadaMs,
                TtlRestante = paso.TtlRestante
            }).ToList()
        };
}