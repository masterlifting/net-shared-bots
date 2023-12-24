using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotMessage : IBotMessage
{
    public string ChatId { get; init; } = null!;
    public string Data { get; init; } = null!;
}
