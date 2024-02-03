using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots.Abstractions.Models.Bot;

public sealed record Chat(string Id);
public sealed record Message
{
    public ResponseMessageBehavior ResponseBehavior { get; set; } = ResponseMessageBehavior.New;

    public Message(int? id, Chat chat)
    {
        Id = id;
        Chat = chat;
    }

    public int? Id { get; }
    public Chat Chat { get; }
}

public sealed record Text(string Value);
public sealed record TextEventArgs(Message Message, Text Text);

public sealed record Command
{
    public string Name { get; set; }

    public Command(Guid id, string name, Dictionary<string, string> parameters)
    {
        Id = id;
        Name = name;
        Parameters = parameters;
    }

    public Guid Id { get; init; }
    public Dictionary<string, string> Parameters { get; init; }
}
