using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Models.Request;

public sealed record TextEventArgs(Chat Chat, Text Text);
public sealed record PhotoEventArgs(Chat Chat, IReadOnlyCollection<Photo> Photos);
public sealed record AudioEventArgs(Chat Chat, Audio Audio);
public sealed record VideoEventArgs(Chat Chat, Video Video);
public sealed record VoiceEventArgs(Chat Chat, Voice Voice);
public sealed record DocumentEventArgs(Chat Chat, Document Document);
public sealed record LocationEventArgs(Chat Chat, Location Location);
public sealed record ContactEventArgs(Chat Chat, Contact Contact);
public sealed record ExceptionEventArgs(Chat Chat, Exception Exception);
