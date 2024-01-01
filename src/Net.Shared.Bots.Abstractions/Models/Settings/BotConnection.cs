using System.ComponentModel.DataAnnotations;

namespace Net.Shared.Bots.Abstractions.Models.Settings;
public sealed record BotConnection
{
    public const string SectionName = "BotConnection";
    [Required]
    public string TokenVariableName { get; init; } = null!;
}
