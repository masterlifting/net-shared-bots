namespace Net.Shared.Bots.Abstractions.Interfaces
{
    public interface IBotCommand
    {
        List<string> Commands { get; }
        Dictionary<string, string> Parameters { get; }
    }
}
