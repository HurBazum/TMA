namespace TMA.UI.Infrastructure.Commands.SpecialCommands;

public class PropertyAccessor<T>
{
    public Func<T> Getter { get; set; } = null!;
    public Action<T> Setter { get; set; } = null!;
}
