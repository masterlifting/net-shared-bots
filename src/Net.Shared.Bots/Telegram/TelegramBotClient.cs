using System.Collections.Immutable;

using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(ILogger<TelegramBotClient> logger, ITelegramBotClient client, IBotService service) : IBotClient
{
    private readonly ILogger _logger = logger;
    private readonly IBotService _service = service;
    private readonly ITelegramBotClient _client = client;

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

    Task HandleReceivedMessage(ITelegramBotClient client, Update? update, CancellationToken cToken)
    {
        ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

        Task result = update.Type switch
        {
            UpdateType.Message => HandleMessage(update.Message, _service),
            UpdateType.EditedMessage => HandleMessage(update.Message, _service),
            UpdateType.ChannelPost => HandleMessage(update.Message, _service),
            UpdateType.EditedChannelPost => HandleMessage(update.Message, _service),
            UpdateType.CallbackQuery => HandleMessage(update.CallbackQuery?.Message, _service),
            _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
        };

        return result;

        static Task HandleMessage(Message? message, IBotService service)
        {
            ArgumentNullException.ThrowIfNull(message, "Received data was not recognized.");

            return message.Type switch
            {
                MessageType.Text => !string.IsNullOrWhiteSpace(message.Text) 
                    ? service.HandleText(message.Chat.Id.ToString(), message.Text)
                    : throw new InvalidOperationException("Text is required."),
                MessageType.Photo => message.Photo is not null 
                    ? service.HandlePhoto(message.Chat.Id.ToString(), message.Photo.Select(x => (x.FileId, x.FileSize)).ToImmutableArray())
                    : throw new InvalidOperationException("Photo is required."),
                MessageType.Audio => message.Audio is not null
                    ? service.HandleAudio(message.Chat.Id.ToString(), message.Audio.FileId, message.Audio.FileSize, message.Audio.MimeType, message.Audio.Title)
                    : throw new InvalidOperationException("Audio is required."),
                MessageType.Video => message.Video is not null 
                    ? service.HandleVideo(message.Chat.Id.ToString(), message.Video.FileId, message.Video.FileSize, message.Video.MimeType, message.Video.FileName)
                    : throw new InvalidOperationException("Video is required."),
                MessageType.Voice => message.Voice is not null 
                    ? service.HandleVoice(message.Chat.Id.ToString(), message.Voice.FileId, message.Voice.FileSize, message.Voice.MimeType)
                    : throw new InvalidOperationException("Voice is required."),
                MessageType.Document => message.Document is not null 
                    ? service.HandleDocument(message.Chat.Id.ToString(), message.Document.FileId, message.Document.FileSize, message.Document.MimeType, message.Document.FileName)
                    : throw new InvalidOperationException("Document is required."),
                MessageType.Location => message.Location is not null
                    ? service.HandleLocation(message.Chat.Id.ToString(), message.Location.Latitude, message.Location.Longitude)
                    : throw new InvalidOperationException("Location is required."),
                MessageType.Contact => message.Contact is not null
                    ? service.HandleContact(message.Chat.Id.ToString(), message.Contact.PhoneNumber, message.Contact.FirstName, message.Contact.LastName)
                    : throw new InvalidOperationException("Contact is required."),
                _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
            };
        }
    }
    Task HandleReceivedMessageError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _logger.LogError(exception, "Received message error.");
        return Task.CompletedTask;
    }
    Task HandleSendingMessage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
