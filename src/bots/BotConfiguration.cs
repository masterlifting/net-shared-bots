using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots;

public sealed class BotConfiguration(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public ServiceLifetime ClientLifetime { get; init; } = ServiceLifetime.Singleton;
    internal bool IsSetCommandsStore { get; private set; }

    public void AddCommandsStore<T>() where T : class, IBotCommandsStore
    {
        Services.AddTransient<IBotCommandsStore, T>();
        IsSetCommandsStore = true;
    }
}
