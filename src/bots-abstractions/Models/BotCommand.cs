namespace Net.Shared.Bots.Abstractions.Models;

public sealed record BotCommand(Guid Id, string Name, Dictionary<string, string> Parameters);
