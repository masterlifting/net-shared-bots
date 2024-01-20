using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots.Abstractions.Interfaces;

public interface IBotCommandsStore
{
    Task Create(string chatId, Command command, CancellationToken cToken);
    Task<Command> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken);
    Task Update(string chatId, Guid commandId, Command command, CancellationToken cToken);
    Task Delete(string chatId, Guid commandId, CancellationToken cToken);
    Task Clear(string chatId, CancellationToken cToken);
    
    Task<Command> Get(string chatId, Guid commandId, CancellationToken cToken);
    Task<Command[]> Get(string chatId, CancellationToken cToken);
}
