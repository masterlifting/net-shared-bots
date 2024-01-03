namespace Net.Shared.Bots.Abstractions.Models;

public sealed record ButtonsEventArgs(string ChatId, Buttons Buttons);
public sealed record WebAppEventArgs(string ChatId, WebApp WebApp);
