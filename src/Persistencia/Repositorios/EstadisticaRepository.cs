using Aplicacion.Interfaces;
using Dapper;

namespace Persistencia.Repositorios;

public class EstadisticaRepository : IEstadisticaRepository
{
    private readonly IDbConnectionFactory _factory;

    public EstadisticaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IEnumerable<object> ObtenerEstadisticas()
    {
        using var connection = _factory.CrearConexionDesarrollo();

        return connection.Query(
            "sp_estadisticas",
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
