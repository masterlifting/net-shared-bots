using System.Collections.Concurrent;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots;

public sealed class BotCommandsCache : IBotCommandsStore
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, BotCommand>> _storage = new();

    public Task<BotCommand> GetCommand(string chatId, Guid commandId, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var value))
            if(value.TryGetValue(commandId, out var command))
                return Task.FromResult(command);

        throw new KeyNotFoundException($"The command with id '{commandId}' is not found.");
    }
    public Task<Guid> SetCommand(string chatId, BotCommand command, CancellationToken cToken)
    {
        var commandId = Guid.NewGuid();

        if (_storage.TryGetValue(chatId, out var value))
        {
            value[commandId] = command;
        }
        else
        {
            _storage.TryAdd(chatId, new() 
            { 
                { commandId, command } 
            });
        }

        return Task.FromResult(commandId);
    }
    public Task ClearCommands(string chatId, CancellationToken cToken)
    {
        return Task.Run(() => _storage.TryRemove(chatId, out _));
    }
}
