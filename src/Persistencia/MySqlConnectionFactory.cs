using System.Data;
using MySqlConnector;

namespace Datos_en_Transito.Persistencia;

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _cadenaAdmin;
    private readonly string _cadenaInvitado;

    public MySqlConnectionFactory(string cadenaAdmin, string cadenaInvitado)
    {
        _cadenaAdmin = cadenaAdmin;
        _cadenaInvitado = cadenaInvitado;
    }

    public IDbConnection CrearConexionEscritura()
        => new MySqlConnection(ConClave(_cadenaAdmin, "DER_ADMIN_PASSWORD"));

    public IDbConnection CrearConexionLectura()
        => new MySqlConnection(ConClave(_cadenaInvitado, "DER_INVITADO_PASSWORD"));

    private static string ConClave(string cadena, string variable)
    {
        var clave = Environment.GetEnvironmentVariable(variable)
            ?? throw new InvalidOperationException($"Falta la variable de entorno {variable}.");
        return new MySqlConnectionStringBuilder(cadena) { Password = clave }.ConnectionString;
    }
}