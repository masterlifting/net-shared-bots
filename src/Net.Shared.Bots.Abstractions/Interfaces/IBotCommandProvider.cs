
namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandProvider
{
    bool TryGetCommand(string chatId, Guid guid, out string value);
}
