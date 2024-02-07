using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots;

public sealed class BotConfiguration(IServiceCollection services)
{
    private readonly IServiceCollection _services = services;

    public ServiceLifetime ClientLifetime { get; set; } = ServiceLifetime.Singleton;
    internal bool IsSetCommandsStore { get; private set; }

    public void AddCommandsStore<T>() where T : class, IBotCommandsStore
    {
        _services.AddTransient<IBotCommandsStore, T>();
        IsSetCommandsStore = true;
    }
}
