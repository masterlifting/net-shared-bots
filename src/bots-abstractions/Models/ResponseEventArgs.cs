using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Models.Response;

public sealed record ButtonsEventArgs(Message Message, Buttons Buttons);
public sealed record WebAppEventArgs(Message Message, WebApps WebApps);
