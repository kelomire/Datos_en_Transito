using Aplicacion.Entidades;
using Aplicacion.Interfaces;

namespace Aplicacion.Servicios;

public class DispositivoService
{
    private readonly IDispositivoRepository _repository;

    public DispositivoService(IDispositivoRepository repository)
    {
        _repository = repository;
    }

    public void Registrar(DispositivoRed dispositivo)
    {
        _repository.Agregar(dispositivo);
    }

    public IEnumerable<DispositivoRed> Listar()
    {
        return _repository.ObtenerTodos();
    }

    public DispositivoRed? Obtener(int id)
    {
        return _repository.ObtenerPorId(id);
    }
}
