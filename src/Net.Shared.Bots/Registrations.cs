using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Telegram;

namespace Net.Shared.Bots;

public static class Registrations
{
    public static IServiceCollection AddTelegramBot<TRequest, TResponse>(this IServiceCollection services, Action<TelegramBotConfiguration>? options = null) 
        where TRequest : class, IBotRequestService 
        where TResponse : class, IBotResponseService
    {
        var configuration = new TelegramBotConfiguration();
        
        if(options is not null)
            options(configuration);
      
        if(!configuration.IsSetCommandsStore)
            services.AddSingleton<IBotCommandsStore, BotCommandsCache>();

        switch(configuration.ClientLifetime)
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


        services.AddTransient<IBotRequestService, TRequest>();
        services.AddTransient<IBotResponseService, TResponse>();

        return services;
    }
}
