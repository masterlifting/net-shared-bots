using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Telegram;

namespace Net.Shared.Bots;

public static class Registrations
{
    public static void AddTelegramBot<T>(this IServiceCollection services) where T : class, IBotService
    {
        services.AddSingleton<IBotClient, TelegramBotClient>();
        services.AddTransient<IBotService, T>();
    }
}
