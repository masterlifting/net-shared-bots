using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Models.Request;

public sealed record PhotoEventArgs(Message Message, IReadOnlyCollection<Photo> Photos);
public sealed record AudioEventArgs(Message Message, Audio Audio);
public sealed record VideoEventArgs(Message Message, Video Video);
public sealed record VoiceEventArgs(Message Message, Voice Voice);
public sealed record DocumentEventArgs(Message Message, Document Document);
public sealed record LocationEventArgs(Message Message, Location Location);
public sealed record ContactEventArgs(Message Message, Contact Contact);
