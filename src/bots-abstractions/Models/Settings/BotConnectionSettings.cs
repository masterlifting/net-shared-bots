using System.ComponentModel.DataAnnotations;

namespace Net.Shared.Bots.Abstractions.Models.Settings;
public sealed record BotConnectionSettings
{
    public const string SectionName = "BotConnection";
    [Required] public string AdminId { get; init; } = null!;
    [Required] public string Token { get; init; } = null!;
}
