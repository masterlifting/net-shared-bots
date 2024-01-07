using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotResponse
{ 
    Task Create(string chatId, BotCommand command, CancellationToken cToken);
}
