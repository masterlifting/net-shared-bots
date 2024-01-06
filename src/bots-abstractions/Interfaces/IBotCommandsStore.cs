using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandsStore
{
    Task<BotCommand> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken);
    Task Update(string chatId, Guid commandId, BotCommand command, CancellationToken cToken);
    Task Delete(string chatId, Guid commandId, CancellationToken cToken);
    Task Clear(string chatId, CancellationToken cToken);
    
    Task<BotCommand> Get(string chatId, Guid commandId, CancellationToken cToken);
    Task<BotCommand[]> Get(string chatId, CancellationToken cToken);
}
