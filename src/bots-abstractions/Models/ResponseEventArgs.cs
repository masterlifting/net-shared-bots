using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Models.Response;

public sealed record MessageEventArgs(Chat Chat, Message Message);
public sealed record ButtonsEventArgs(Chat Chat, Buttons Buttons);
public sealed record WebAppEventArgs(Chat Chat, WebApp WebApp);
