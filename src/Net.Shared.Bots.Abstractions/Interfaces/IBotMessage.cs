namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotMessage
{
    string ChatId { get; init; }
    string Data { get; init; }
}
