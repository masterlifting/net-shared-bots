using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Models.Response;

public sealed record Buttons(string Name, Dictionary<string, string> Data, byte Columns = 0);
public sealed record WebApp(string Name, Dictionary<string, Uri> Data, byte Columns = 0);
public sealed record Result(Message Message);
