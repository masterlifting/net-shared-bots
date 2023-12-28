using System.Text;

namespace Net.Shared.Bots.Abstractions.Models;

public sealed record BotCommand
{
    public string Name { get; }
    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    private readonly Dictionary<string, string> _parameters = new(10);

    public BotCommand(string input)
    {
        var inputSpan = input.AsSpan();

        if (!inputSpan.StartsWith("/"))
            throw new InvalidOperationException($"Command '{input}' is not valid.");

        var parametersStartIndex = inputSpan.IndexOf('?');

        var command = parametersStartIndex > 1 ? inputSpan[..parametersStartIndex] : inputSpan;

        if (command.Length < 3)
            throw new InvalidOperationException($"Command '{input}' is not valid.");

        Name = command[1..].ToString();

        var parametersSpan = parametersStartIndex > -1 ? inputSpan[(parametersStartIndex + 1)..] : [];

        if(parametersSpan.Length == 0)
            return;

        foreach (var item in parametersSpan.ToString().Split('&'))
        {
            var keyValue = item.Split('=');

            _parameters[keyValue[0]] = keyValue.Length == 2
                ? keyValue[1]
                : throw new InvalidOperationException($"Command parameter '{item}' is not valid.");
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append('/');

        builder.Append(Name);

        if (Parameters.Count > 0)
        {
            builder.Append('?');

            foreach (var (key, value) in Parameters)
            {
                builder.Append(key);
                builder.Append('=');
                builder.Append(value);
                builder.Append('&');
            }

            builder.Remove(builder.Length - 1, 1);
        }

        return builder.ToString();
    }
}
