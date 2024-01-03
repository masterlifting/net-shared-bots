namespace Net.Shared.Bots.Abstractions.Models;

public sealed record Buttons(string Name, byte Columns, Dictionary<string, string> Data);
public sealed record WebApp(string Name, Uri Uri);
