using Persistencia.Entidades;
using Persistencia.Interfaces;

namespace Aplicacion.Servicios;

public class DispositivoService
{
    private readonly IDispositivoRepository _repository;

    public DispositivoService(IDispositivoRepository repository)
    {
        _repository = repository;
    }

    public int Registrar(DispositivoRed dispositivo)
    {
        var id = _repository.Agregar(dispositivo);
        dispositivo.Id = id;
        return id;
    }

    public int Registrar(string nombre, string tipo, int latenciaMs)
        => Registrar(CrearDispositivo(0, nombre, tipo, latenciaMs));

    public IEnumerable<DispositivoRed> Listar()
    {
        return _repository.ObtenerTodos();
    }

    public DispositivoRed? Obtener(int id)
    {
        return _repository.ObtenerPorId(id);
    }

    public bool Actualizar(int id, string nombre, string tipo, int latenciaMs)
        => _repository.Actualizar(CrearDispositivo(id, nombre, tipo, latenciaMs));

    public ResultadoEliminacionDispositivo Eliminar(int id)
        => _repository.Eliminar(id);

    private static DispositivoRed CrearDispositivo(int id, string nombre, string tipo, int latenciaMs)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (latenciaMs < 0)
            throw new ArgumentOutOfRangeException(nameof(latenciaMs), "La latencia debe ser mayor o igual a cero.");

        return tipo.Trim().ToLowerInvariant() switch
        {
            "router" => new Router(id, nombre.Trim(), latenciaMs),
            "switch" => new Switch(id, nombre.Trim(), latenciaMs),
            "firewall" => new Firewall(id, nombre.Trim(), latenciaMs),
            "accesspoint" => new AccessPoint(id, nombre.Trim(), latenciaMs),
            _ => throw new ArgumentException($"Tipo de dispositivo no soportado: {tipo.Trim()}.", nameof(tipo))
        };
    }
}
