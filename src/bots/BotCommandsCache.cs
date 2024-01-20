using System.Collections.Concurrent;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;

namespace Net.Shared.Bots;

public sealed class BotCommandsCache : IBotCommandsStore
{
    private readonly ConcurrentDictionary<string, Dictionary<Guid, Command>> _storage = new();

    public Task Create(string chatId, Command command, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var commands))
        {
            commands[command.Id] = commands.ContainsKey(command.Id)
                ? throw new InvalidOperationException($"The command '{command.Id}' for chat '{chatId}' already exists.")
                : command;
        }
        else
            _storage.TryAdd(chatId, new Dictionary<Guid, Command> { { command.Id, command } });

        return Task.CompletedTask;
    }
    public async Task<Command> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken)
    {
        var command = new Command(Guid.NewGuid(), Name, Parameters);

        await Create(chatId, command, cToken);

        return command;
    }
    public Task Update(string chatId, Guid commandId, Command command, CancellationToken cToken)
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
    
    public Task<Command> Get(string chatId, Guid commandId, CancellationToken cToken)
    {
        if (_storage.TryGetValue(chatId, out var value))
            if(value.TryGetValue(commandId, out var command))
                return Task.FromResult(command);

        throw new KeyNotFoundException($"The command '{commandId}' for chat '{chatId}' is not found.");
    }
    public Task<Command[]> Get(string chatId, CancellationToken cToken) => 
        Task.FromResult(_storage.TryGetValue(chatId, out var commands)
            ? [.. commands.Values]
            : Array.Empty<Command>());

}
