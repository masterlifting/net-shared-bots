using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotResponse
{
    Task Create(Chat chat, Command command, CancellationToken cToken);
    Task Create(string chatId, Command command, CancellationToken cToken);
}
