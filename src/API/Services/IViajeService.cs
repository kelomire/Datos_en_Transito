using Datos_en_Transito.Api.DTOs;

namespace Datos_en_Transito.Api.Services;

public interface IViajeService
{
    IEnumerable<ViajeRespuesta> ListarTodos();
    ViajeRespuesta? BuscarPorId(int id);
    ViajeRespuesta Crear(CrearViajeSolicitud datos);
}
