using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Bots.Telegram;

namespace Net.Shared.Bots;

public static class Registrations
{
    public static IServiceCollection AddTelegramBot<TResponse>(this IServiceCollection services, Action<BotConfiguration> configure)
        where TResponse : class, IBotResponse
    {
        services
            .AddOptions<TelegramBotConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(TelegramBotConnectionSettings.SectionName)
                    .Bind(settings);
            });

        var botConfiguration = new BotConfiguration(services);

        configure(botConfiguration);

        switch (botConfiguration.ClientLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IBotClient, TelegramBotClient>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<IBotClient, TelegramBotClient>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IBotClient, TelegramBotClient>();
                break;
        }

        services.AddTransient<IBotResponse, TResponse>();

        if (!botConfiguration.IsSetCommandsStore)
            services.AddSingleton<IBotCommandsStore, BotCommandsCache>();

        services.AddTransient<IBotRequest, BotRequest>();

        return services;
    }
}
