using Net.Shared.Bots.Abstractions.Models.Bot;

using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Abstractions.Models.Response;

public sealed record Buttons(string Name, Dictionary<string, string> Data, ResponseButtonsColumns Columns = ResponseButtonsColumns.Auto);
public sealed record WebApps(string Name, Dictionary<string, Uri> Data, ResponseButtonsColumns Columns = ResponseButtonsColumns.Auto);
public sealed record Result(Message Message);
