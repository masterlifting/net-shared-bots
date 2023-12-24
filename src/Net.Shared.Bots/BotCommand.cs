using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots;

public readonly struct BotCommand : IBotCommand
{
    public List<string> Commands { get; } = [];
    public Dictionary<string, string> Parameters { get; } = [];

    public BotCommand(string input)
    {
        var span = input.AsSpan();
        var delimiterPos = span.IndexOf('?');

        var commands = delimiterPos > -1 ? span[..delimiterPos] : span;
        var parameters = delimiterPos > -1 ? span[(delimiterPos + 1)..] : [];

        foreach (var item in commands.ToString().Split('/'))
        {
            Commands.Add(item);
        }

        if(Commands.Count == 0)
        {
            throw new InvalidOperationException($"Command {input} is not valid.");
        }

        foreach (var item in parameters.ToString().Split('&'))
        {
            var keyValue = item.Split('=');

            if (keyValue.Length == 2)
            {
                Parameters[keyValue[0]] = keyValue[1];
            }
            else
            {
                throw new InvalidOperationException($"Parameter {item} is not valid.");
            }
        }
    }
}
