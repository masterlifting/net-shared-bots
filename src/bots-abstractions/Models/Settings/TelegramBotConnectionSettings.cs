using System.ComponentModel.DataAnnotations;

namespace Net.Shared.Bots.Abstractions.Models.Settings;
public sealed record TelegramBotConnectionSettings
{
    public const string SectionName = "TelegramBotConnection";
    [Required] public string AdminChatId { get; init; } = null!;
    [Required] public string Token { get; init; } = null!;
}
