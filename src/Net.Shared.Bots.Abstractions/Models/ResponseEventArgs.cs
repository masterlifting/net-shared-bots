namespace Net.Shared.Bots.Abstractions.Models;

public sealed record MessageEventArgs(string ChatId, Message Message);
public sealed record ButtonsEventArgs(string ChatId, Buttons Buttons);
public sealed record WebAppEventArgs(string ChatId, WebApp WebApp);
