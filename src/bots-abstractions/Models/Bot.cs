namespace Net.Shared.Bots.Abstractions.Models.Bot;
public sealed record Message(string Id);
public sealed record Chat(string Id, Message Message);
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
