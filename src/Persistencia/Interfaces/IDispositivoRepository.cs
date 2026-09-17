using Persistencia.Entidades;

namespace Persistencia.Interfaces;

public interface IDispositivoRepository
{
    IEnumerable<DispositivoRed> ObtenerTodos();
    DispositivoRed? ObtenerPorId(int id);
    void Agregar(DispositivoRed dispositivo);
}
