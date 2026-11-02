namespace Persistencia.Entidades;

public class Simulacion
{
    public int Id { get; set; }
    public PaqueteRed Paquete { get; }
    public List<DispositivoRed> Dispositivos { get; }
    public List<ResultadoProcesamiento> Resultados { get; } = new();
    public int latenciaMsTotal { get; private set; }

    public Simulacion(PaqueteRed paquete, IEnumerable<DispositivoRed> dispositivos)
    {
        Paquete = paquete ?? throw new ArgumentNullException(nameof(paquete));
        Dispositivos = dispositivos?.ToList() ?? throw new ArgumentNullException(nameof(dispositivos));
    }

    public void Ejecutar()
    {
        foreach (var dispositivo in Dispositivos)
        {
            if (Paquete.Descartado)
                break;

            var resultado = dispositivo.Procesar(Paquete);
            Resultados.Add(resultado);
            latenciaMsTotal += resultado.latenciaMs;
        }

        if (!Paquete.Descartado)
            Paquete.Entregado = true;
    }
}
