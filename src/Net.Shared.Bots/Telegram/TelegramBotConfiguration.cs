using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots.Telegram;

public class TelegramBotConfiguration
{
    internal bool  IsSetCommandsStore { get; private set; }
    public ServiceLifetime ClientLifetime { get; set; } = ServiceLifetime.Singleton;
    public void AddCommandsStore<T>(IServiceCollection services) where T : class, IBotCommandsStore
    {
        services.AddTransient<IBotCommandsStore, T>();
        IsSetCommandsStore = true;
    }
}
