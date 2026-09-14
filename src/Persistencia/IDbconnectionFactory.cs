using MySqlConnector;
using System.Data;

namespace Persistencia;

public interface IDbConnectionFactory
{
    IDbConnection CrearConexionAdministrador();
    IDbConnection CrearConexionDesarrollo();
}
