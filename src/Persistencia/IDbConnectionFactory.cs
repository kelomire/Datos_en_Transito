using System.Data;

namespace Datos_en_Transito.Persistencia;

public interface IDbConnectionFactory
{
    IDbConnection CrearConexionEscritura(); // usuario administrador
    IDbConnection CrearConexionLectura();   // usuario invitado (solo SELECT)
}