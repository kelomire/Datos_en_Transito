using Datos_en_Transito.Api.DTOs;

namespace Datos_en_Transito.Api.Services;

public class ViajeService : IViajeService
{
    private static readonly List<ViajeRespuesta> _almacen = new();
    private static int _siguienteId = 1;

    public IEnumerable<ViajeRespuesta> ListarTodos() => _almacen;

    public ViajeRespuesta? BuscarPorId(int id)
        => _almacen.FirstOrDefault(v => v.Id == id);

    public ViajeRespuesta Crear(CrearViajeSolicitud datos)
    {
        var nuevo = new ViajeRespuesta(
            _siguienteId++,
            datos.Origen,
            datos.Destino,
            datos.FechaSalida
        );
        _almacen.Add(nuevo);
        return nuevo;
    }
}
