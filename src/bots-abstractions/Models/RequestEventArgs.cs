namespace Net.Shared.Bots.Abstractions.Models;

public sealed record TextEventArgs(BotChat Chat, Text Text);
public sealed record PhotoEventArgs(BotChat Chat, IReadOnlyCollection<Photo> Photos);
public sealed record AudioEventArgs(BotChat Chat, Audio Audio);
public sealed record VideoEventArgs(BotChat Chat, Video Video);
public sealed record VoiceEventArgs(BotChat Chat, Voice Voice);
public sealed record DocumentEventArgs(BotChat Chat, Document Document);
public sealed record LocationEventArgs(BotChat Chat, Location Location);
public sealed record ContactEventArgs(BotChat Chat, Contact Contact);
public sealed record ExceptionEventArgs(BotChat Chat, Exception Exception);
