﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Bots.Telegram;

namespace Net.Shared.Bots;

public static class Registrations
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration, Action<BotConfiguration> configure)
    {
        var botConnectionSettings = 
            configuration.GetSection(BotConnectionSettings.SectionName) 
            ?? throw new ArgumentNullException($"The configuration section '{BotConnectionSettings.SectionName}' is not found.");
        
        services.Configure<BotConnectionSettings>(botConnectionSettings);

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

        if(!botConfiguration.IsSetRequestHandler)
            throw new NotImplementedException($"Request handler should be implemented {nameof(IBotRequestService)} interface and set by the configuration of the bot.");

        if (!botConfiguration.IsSetResponseHandler)
            throw new NotImplementedException($"Response handler should be implemented {nameof(IBotResponseService)} interface and set by the configuration of the bot.");

        if (!botConfiguration.IsSetCommandsStore)
            services.AddSingleton<IBotCommandsStore, BotCommandsCache>();

        return services;
    }
}
