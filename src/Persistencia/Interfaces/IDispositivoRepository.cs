using Persistencia.Entidades;

namespace Persistencia.Interfaces;

public interface IDispositivoRepository
{
    IEnumerable<DispositivoRed> ObtenerTodos();
    DispositivoRed? ObtenerPorId(int id);
    int Agregar(DispositivoRed dispositivo);
    bool Actualizar(DispositivoRed dispositivo);
    ResultadoEliminacionDispositivo Eliminar(int id);
}
