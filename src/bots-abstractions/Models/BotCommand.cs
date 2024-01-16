namespace Net.Shared.Bots.Abstractions.Models;

public sealed record BotMessage(string Id);
public sealed record BotChat(string Id, BotMessage Message);
public sealed record BotCommand(Guid Id, string Name, Dictionary<string, string> Parameters);
