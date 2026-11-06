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
            ?? Environment.GetEnvironmentVariable(variable == "DER_ADMIN_PASSWORD"
                ? "DB_PASSWORD_ADMIN"
                : "DB_PASSWORD_READONLY")
            ?? throw new InvalidOperationException(
                $"Falta la variable de entorno {variable} (o su alternativa DB_PASSWORD correspondiente).");
        return new MySqlConnectionStringBuilder(cadena) { Password = clave }.ConnectionString;
    }
}