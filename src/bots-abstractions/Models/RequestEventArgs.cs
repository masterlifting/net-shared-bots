namespace Net.Shared.Bots.Abstractions.Models;

public sealed record TextEventArgs(string ChatId, Text Text);
public sealed record PhotoEventArgs(string ChatId, IReadOnlyCollection<Photo> Photos);
public sealed record AudioEventArgs(string ChatId, Audio Audio);
public sealed record VideoEventArgs(string ChatId, Video Video);
public sealed record VoiceEventArgs(string ChatId, Voice Voice);
public sealed record DocumentEventArgs(string ChatId, Document Document);
public sealed record LocationEventArgs(string ChatId, Location Location);
public sealed record ContactEventArgs(string ChatId, Contact Contact);
public sealed record ExceptionEventArgs(string ChatId, Exception Exception);
