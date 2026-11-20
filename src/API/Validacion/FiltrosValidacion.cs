namespace API.Validacion;

public static class FiltrosValidacion
{
    public static string? CanonicalizarTipo(string tipo) =>
        new[] { "Router", "Switch", "Firewall", "AccessPoint" }
            .FirstOrDefault(t => string.Equals(t, tipo.Trim(), StringComparison.OrdinalIgnoreCase));
}
