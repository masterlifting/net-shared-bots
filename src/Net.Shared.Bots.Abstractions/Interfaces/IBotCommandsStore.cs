using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandsStore
{
    Task<BotCommand> Get(string chatId, Guid commandId, CancellationToken cToken);
    Task<Guid> Create(string chatId, BotCommand command, CancellationToken cToken);
    Task Update(string chatId, Guid commandId, BotCommand command, CancellationToken cToken);
    Task Delete(string chatId, Guid commandId, CancellationToken cToken);
    Task Clear(string chatId, CancellationToken cToken);
}
