using Persistencia.Entidades;

namespace Persistencia.Interfaces;

public interface ISimulacionRepository
{
    void Registrar(Simulacion simulacion);
    Simulacion? ObtenerPorId(int id);
    IEnumerable<Simulacion> ObtenerTodas();
}
