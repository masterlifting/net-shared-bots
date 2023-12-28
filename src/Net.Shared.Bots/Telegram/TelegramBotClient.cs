using System.Collections.Immutable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using ExternalTelegramBotClient = Telegram.Bot.TelegramBotClient;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(ILogger<TelegramBotClient> logger, IServiceScopeFactory scopeFactory) : IBotClient
{
    private readonly ILogger _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ITelegramBotClient _client = new ExternalTelegramBotClient("");

    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        await _client.SetWebhookAsync(uri.ToString(), cancellationToken: cToken);
    }
    public async Task Listen(CancellationToken cToken)
    {
        await _client.DeleteWebhookAsync(true, cToken);
        
        var options = new ReceiverOptions
        {
        };

        _client.StartReceiving(HandleReceivedMessage, HandleReceivedMessageError, options, cToken);
    }
    public async Task Send(IBotMessage message, CancellationToken cToken)
    {
        await HandleSendingMessage(message, cToken);
    }
    public async Task Receive(string data, CancellationToken cToken)
    {
        var update = JsonConvert.DeserializeObject<Update>(data);

        await HandleReceivedMessage(_client, update, cToken);
    }

    public async Task<byte[]> LoadFile(string fileId, CancellationToken cToken)
    {
        using var stream = new MemoryStream();
        var file = await _client.GetInfoAndDownloadFileAsync(fileId, stream, cancellationToken:cToken);
        return stream.ToArray();
    }
    public Task SendButtons(string chatId, Dictionary<string, string> clientButtons, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task SendWebForm(string chatId, object data, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task SendText(string chatId, string v, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    private Task HandleReceivedMessage(ITelegramBotClient client, Update? update, CancellationToken cToken)
    {
        ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

        Task result = update.Type switch
        {
            UpdateType.Message => HandleMessage(update.Message, cToken),
            UpdateType.EditedMessage => HandleMessage(update.Message, cToken),
            UpdateType.ChannelPost => HandleMessage(update.Message, cToken),
            UpdateType.EditedChannelPost => HandleMessage(update.Message, cToken),
            UpdateType.CallbackQuery => HandleMessage(update.CallbackQuery?.Message, cToken),
            _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
        };

        return result;

        Task HandleMessage(Message? message, CancellationToken cToken)
        {
            ArgumentNullException.ThrowIfNull(message, "Received data was not recognized.");

            return message.Type switch
            {
                MessageType.Text => !string.IsNullOrWhiteSpace(message.Text) 
                    ? OnTextHandler(new(message.Chat.Id.ToString(), new(message.Text)), cToken)
                    : throw new InvalidOperationException("Text is required."),
                MessageType.Photo => message.Photo is not null 
                    ? OnPhotoHandler(new(message.Chat.Id.ToString(), message.Photo.Select(x => new Photo(x.FileId, x.FileSize)).ToImmutableArray()), cToken)
                    : throw new InvalidOperationException("Photo is required."),
                MessageType.Audio => message.Audio is not null
                    ? OnAudioHandler(new(message.Chat.Id.ToString(), new(message.Audio.FileId, message.Audio.FileSize, message.Audio.Title, message.Audio.MimeType)), cToken)
                    : throw new InvalidOperationException("Audio is required."),
                MessageType.Video => message.Video is not null 
                    ? OnVideoHandler(new(message.Chat.Id.ToString(), new(message.Video.FileId, message.Video.FileSize, message.Video.FileName, message.Video.MimeType)), cToken)
                    : throw new InvalidOperationException("Video is required."),
                MessageType.Voice => message.Voice is not null 
                    ? OnVoiceHandler(new(message.Chat.Id.ToString(), new(message.Voice.FileId, message.Voice.FileSize, message.Voice.MimeType)), cToken)
                    : throw new InvalidOperationException("Voice is required."),
                MessageType.Document => message.Document is not null 
                    ? OnDocumentHandler(new(message.Chat.Id.ToString(), new(message.Document.FileId, message.Document.FileSize, message.Document.FileName, message.Document.MimeType)), cToken)
                    : throw new InvalidOperationException("Document is required."),
                MessageType.Location => message.Location is not null
                    ? OnLocationHandler(new(message.Chat.Id.ToString(), new(message.Location.Latitude, message.Location.Longitude)), cToken)
                    : throw new InvalidOperationException("Location is required."),
                MessageType.Contact => message.Contact is not null
                    ? OnContactHandler(new(message.Chat.Id.ToString(), new(message.Contact.PhoneNumber, message.Contact.FirstName, message.Contact.LastName)), cToken)
                    : throw new InvalidOperationException("Contact is required."),
                _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
            };
        }
    }
    private Task HandleReceivedMessageError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _logger.LogError(exception, "Received message error.");
        return Task.CompletedTask;
    }
    private Task HandleSendingMessage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    private Task OnTextHandler(TextEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnTextHandler(args);
        }    

        return Task.CompletedTask;
    }
    private Task OnPhotoHandler(PhotoEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnPhotoHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnAudioHandler(AudioEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnAudioHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnVideoHandler(VideoEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnVideoHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnVoiceHandler(VoiceEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnVoiceHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnDocumentHandler(DocumentEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnDocumentHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnLocationHandler(LocationEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnLocationHandler(args);
        }

        return Task.CompletedTask;
    }
    private Task OnContactHandler(ContactEventArgs args, CancellationToken cToken)
    {
        if(!cToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            return requestService.OnContactHandler(args);
        }

        return Task.CompletedTask;
    }
}
