
namespace Datos_en_Transito.Api.DTOs;

public record CrearViajeSolicitud(
    string Origen,
    string Destino,
    DateTime FechaSalida
)
{
    public bool EsValido()
    {
        return !string.IsNullOrWhiteSpace(Origen)
            && !string.IsNullOrWhiteSpace(Destino)
            && FechaSalida > DateTime.UtcNow;
    }
}

public record ViajeRespuesta(
    int Id,
    string Origen,
    string Destino,
    DateTime FechaSalida
);
