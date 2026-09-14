using MySqlConnector;
using System.Data;

namespace Persistencia;

public class MySqlconnectionFactory : IDbConnectionFactory
{
    private readonly string _admin;
    private readonly string _desarrollo;

    public MySqlconnectionFactory(string admin, string desarrollo)
    {
        _admin = admin;
        _desarrollo = desarrollo;
    }

    public IDbConnection CrearConexionAdministrador()
    {
        return new MySqlConnection(_admin);
    }

    public IDbConnection CrearConexionDesarrollo()
    {
        return new MySqlConnection(_desarrollo);
    }
}
