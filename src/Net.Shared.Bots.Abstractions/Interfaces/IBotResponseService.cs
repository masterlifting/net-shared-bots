using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotResponseService
{ 
    Task CreateResponse(string chatId, BotCommand command, CancellationToken cToken);
}
