using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandsStore
{
    Task<BotCommand> GetCommand(string chatId, Guid commandId, CancellationToken cToken);
    Task<Guid> SetCommand(string chatId, BotCommand command, CancellationToken cToken);
    Task ClearCommands(string chatId, CancellationToken cToken);
}
