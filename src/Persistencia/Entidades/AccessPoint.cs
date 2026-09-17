namespace Persistencia.Entidades;

public class AccessPoint : DispositivoRed
{
    public AccessPoint(int id, string nombre, int latencia = 5) : base(id, nombre, latencia) 
    {}

    public override ResultadoProcesamiento Procesar(PaqueteRed paquete)
    {
        paquete.Ttl--;

        if (paquete.Ttl <= 0)
            paquete.Descartado = true;

        return new ResultadoProcesamiento
        {
            DispositivoId = Id,
            TipoDispositivo = nameof(AccessPoint),
            Latencia = Latencia,
            TtlFinal = paquete.Ttl,
            Entregado = paquete.Entregado,
            Descartado = paquete.Descartado
        };
    }
}
