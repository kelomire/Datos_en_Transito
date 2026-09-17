using Persistencia.Entidades;
using Persistencia.Interfaces;

namespace Aplicacion.Servicios;

public class SimulacionService
{
    private readonly ISimulacionRepository _repository;

    public SimulacionService(ISimulacionRepository repository)
    {
        _repository = repository;
    }

    public Simulacion Ejecutar(PaqueteRed paquete, IEnumerable<DispositivoRed> dispositivos)
    {
        var lista = dispositivos.ToList();

        if (!lista.Any())
            throw new ArgumentException("Debe existir al menos un dispositivo");

        var simulacion = new Simulacion(paquete, lista);
        simulacion.Ejecutar();

        _repository.Registrar(simulacion);

        return simulacion;
    }

    public Simulacion? Obtener(int id)
    {
        return _repository.ObtenerPorId(id);
    }

    public IEnumerable<Simulacion> Listar()
    {
        return _repository.ObtenerTodas();
    }
}
