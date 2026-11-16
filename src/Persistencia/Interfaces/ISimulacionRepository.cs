using Persistencia.Entidades;

namespace Persistencia.Interfaces;

public interface ISimulacionRepository
{
    int Registrar(Simulacion simulacion);
    bool Actualizar(int id, Simulacion simulacion);
    SimulacionPersistida? ObtenerPorId(int id);
    IEnumerable<SimulacionPersistida> ObtenerTodas();
    bool Eliminar(int id);
}
