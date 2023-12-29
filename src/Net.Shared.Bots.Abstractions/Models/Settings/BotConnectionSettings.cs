namespace Net.Shared.Bots.Abstractions.Models.Settings;
public sealed record BotConnectionSettings
{
    public const string SectionName = "BotConnection";
    public string TokenVariableName { get; init; } = null!;
}
