using Aplicacion.Interfaces;

namespace Aplicacion.Servicios;

public class EstadisticaService
{
    private readonly IEstadisticaRepository _repository;

    public EstadisticaService(IEstadisticaRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<object> Obtener()
    {
        return _repository.ObtenerEstadisticas();
    }
}
