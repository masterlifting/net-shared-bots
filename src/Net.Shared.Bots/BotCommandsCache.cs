using System.Collections.Concurrent;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace Net.Shared.Bots;

public sealed class BotCommandsCache : IBotCommandsStore
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, BotCommand>> _storage = new();

    public Task<BotCommand> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken)
    {
        var commandId = Guid.NewGuid();

        var command = new BotCommand(commandId, Name, Parameters);

        if(_storage.TryGetValue(chatId, out var commands))
        {
            if (commands.ContainsKey(commandId))
                throw new InvalidOperationException($"The command '{commandId}' for chat '{chatId}' already exists.");
        }
        else
            _storage.TryAdd(chatId, new Dictionary<Guid, BotCommand> { { commandId, command } });
        
        return Task.FromResult(command);
    }
    public Task Update(string chatId, Guid commandId, BotCommand command, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var commands))
        {
            commands[commandId] = command;
        }
        else
        {
            throw new KeyNotFoundException($"The command '{commandId}' for chat '{chatId}' is not found.");
        }

        return Task.CompletedTask;
    }
    public Task Delete(string chatId, Guid commandId, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var commands))
        {
            commands.Remove(commandId);
        }
        else
        {
            throw new KeyNotFoundException($"The command '{commandId}' for chat '{chatId}' is not found.");
        }

        return Task.CompletedTask;
    }
    public Task Clear(string chatId, CancellationToken cToken)
    {
        return Task.Run(() => _storage.TryRemove(chatId, out _));
    }
    
    public Task<BotCommand> Get(string chatId, Guid commandId, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var value))
            if(value.TryGetValue(commandId, out var command))
                return Task.FromResult(command);

        throw new KeyNotFoundException($"The command '{commandId}' for chat '{chatId}' is not found.");
    }
    public Task<BotCommand[]> Get(string chatId, CancellationToken cToken) => 
        Task.FromResult(_storage.TryGetValue(chatId, out var commands)
            ? [.. commands.Values]
            : Array.Empty<BotCommand>());
}
