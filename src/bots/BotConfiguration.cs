using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots;

public class BotConfiguration(IServiceCollection services)
{
    private readonly IServiceCollection _services = services;

    public ServiceLifetime ClientLifetime { get; init; } = ServiceLifetime.Singleton;

    internal bool IsSetCommandsStore { get; private set; }
    internal bool IsSetRequestHandler { get; private set; }
    internal bool IsSetResponseHandler { get; private set; }

    public void AddCommandsStore<T>() where T : class, IBotCommandsStore
    {
        _services.AddTransient<IBotCommandsStore, T>();
        IsSetCommandsStore = true;
    }
    public void AddRequestHandler<T>() where T : class, IBotRequestService
    {
        _services.AddTransient<IBotRequestService, T>();
        IsSetRequestHandler = true;
    }
    public void AddResponseHandler<T>() where T : class, IBotResponseService
    {
        _services.AddTransient<IBotResponseService, T>();
        IsSetResponseHandler = true;
    }
}
