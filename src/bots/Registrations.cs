using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Bots.Telegram;

namespace Net.Shared.Bots;

public static class Registrations
{
    public static IServiceCollection AddTelegramBot<TResponse>(this IServiceCollection services, Action<BotConfiguration>? configure = null)
        where TResponse : class, IBotResponse
    {
        services
            .AddOptions<BotConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(BotConnectionSettings.SectionName)
                    .Bind(settings);
            })
            .ValidateOnStart()
            .Validate(x => !string.IsNullOrWhiteSpace(x.Token), "Token of Telegram bot should not be empty.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.AdminId), "Admin chat id of Telegram bot should not be empty.");


        var botConfiguration = new BotConfiguration(services);

        configure?.Invoke(botConfiguration);

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
