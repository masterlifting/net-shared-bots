using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using ExternalTelegramBotClient = Telegram.Bot.TelegramBotClient;
using Message = Telegram.Bot.Types.Message;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(
    ILogger<TelegramBotClient> logger,
    IOptions<TelegramBotConnectionSettings> options,
    IServiceScopeFactory scopeFactory) : IBotClient
{
    private readonly ILogger _log = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ITelegramBotClient _client = new ExternalTelegramBotClient(options.Value.Token);

    public async Task Listen(Uri uri, CancellationToken cToken)
    {
        await _client.DeleteWebhookAsync(true, cToken);
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
    public async Task Receive(string data, CancellationToken cToken)
    {
        var update = JsonSerializer.Deserialize<Update>(data);

        await HandleReceivedMessage(_client, update, cToken);
    }

    public async Task<byte[]> LoadFile(string fileId, CancellationToken cToken)
    {
        using var stream = new MemoryStream();
        var file = await _client.GetInfoAndDownloadFileAsync(fileId, stream, cancellationToken: cToken);
        return stream.ToArray();
    }
    
    public async Task SendButtons(ButtonsEventArgs args, CancellationToken cToken)
    {
        var buttonsByColumns = CreateByColumns(args.Buttons.Columns, args.Buttons.Data);

        var result = new InlineKeyboardMarkup(buttonsByColumns);

        await _client.SendTextMessageAsync(args.ChatId, args.Buttons.Name, replyMarkup: result, cancellationToken: cToken);

        static List<InlineKeyboardButton[]> CreateByColumns(byte columns, Dictionary<string, string> data)
        {
            var rowsCount = (int)MathF.Ceiling(data.Count / (float)columns);

            var result = new List<InlineKeyboardButton[]>(rowsCount);

            for (var i = 0; i < data.Count; i += columns)
                result.Add(data
                    .Skip(i)
                    .Take(columns)
                    .Select(x => InlineKeyboardButton.WithCallbackData(x.Value, x.Key))
                    .ToArray());

            return result;
        }
    }
    public async Task SendWebApp(WebAppEventArgs args, CancellationToken cToken)
    {
        var buttonsByColumns = CreateByColumns(args.WebApp.Columns, args.WebApp.Data);

        var result = new InlineKeyboardMarkup(buttonsByColumns);

        await _client.SendTextMessageAsync(args.ChatId, args.WebApp.Name, replyMarkup: result, cancellationToken: cToken);

        static List<InlineKeyboardButton[]> CreateByColumns(byte columns, Dictionary<string, Uri> data)
        {
            var rowsCount = (int)MathF.Ceiling(data.Count / (float)columns);

            var result = new List<InlineKeyboardButton[]>(rowsCount);

            for (var i = 0; i < data.Count; i += columns)
                result.Add(data
                    .Skip(i)
                    .Take(columns)
                    .Select(x => InlineKeyboardButton.WithWebApp(x.Key, new()
                    {
                        Url = x.Value.ToString(),
                    }))
                    .ToArray());

            return result;
        }
    }
    public Task SendMessage(MessageEventArgs args, CancellationToken cToken)
    {
        return _client.SendTextMessageAsync(args.ChatId, args.Message.Text, cancellationToken: cToken);
    }

    private async Task HandleReceivedMessage(ITelegramBotClient client, Update? update, CancellationToken cToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            var result = update.Type switch
            {
                UpdateType.Message => HandleMessage(requestService, update.Message, cToken),
                UpdateType.EditedMessage => HandleMessage(requestService, update.Message, cToken),
                UpdateType.ChannelPost => HandleMessage(requestService, update.Message, cToken),
                UpdateType.EditedChannelPost => HandleMessage(requestService, update.Message, cToken),
                UpdateType.CallbackQuery => !string.IsNullOrWhiteSpace(update.CallbackQuery?.Data)
                    ? OnTextHandler(requestService, new(update.CallbackQuery.From.Id.ToString(), new(update.CallbackQuery.Data)), cToken)
                    : throw new InvalidOperationException("Callback data is required."),
                _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
            };

            await result;
        }
        catch (Exception exception)
        {
            await HandleReceivedMessageError(client, exception, cToken);
        }

        static Task HandleMessage(IBotRequestService requestService, Message? message, CancellationToken cToken)
        {
            ArgumentNullException.ThrowIfNull(message, "Received data was not recognized.");

            return message.Type switch
            {
                MessageType.Text => !string.IsNullOrWhiteSpace(message.Text)
                    ? OnTextHandler(requestService, new(message.Chat.Id.ToString(), new(message.Text)), cToken)
                    : throw new InvalidOperationException("Text is required."),
                MessageType.Photo => message.Photo is not null
                    ? OnPhotoHandler(requestService, new(message.Chat.Id.ToString(), message.Photo.Select(x => new Photo(x.FileId, x.FileSize)).ToImmutableArray()), cToken)
                    : throw new InvalidOperationException("Photo is required."),
                MessageType.Audio => message.Audio is not null
                    ? OnAudioHandler(requestService, new(message.Chat.Id.ToString(), new(message.Audio.FileId, message.Audio.FileSize, message.Audio.Title, message.Audio.MimeType)), cToken)
                    : throw new InvalidOperationException("Audio is required."),
                MessageType.Video => message.Video is not null
                    ? OnVideoHandler(requestService, new(message.Chat.Id.ToString(), new(message.Video.FileId, message.Video.FileSize, message.Video.FileName, message.Video.MimeType)), cToken)
                    : throw new InvalidOperationException("Video is required."),
                MessageType.Voice => message.Voice is not null
                    ? OnVoiceHandler(requestService, new(message.Chat.Id.ToString(), new(message.Voice.FileId, message.Voice.FileSize, message.Voice.MimeType)), cToken)
                    : throw new InvalidOperationException("Voice is required."),
                MessageType.Document => message.Document is not null
                    ? OnDocumentHandler(requestService, new(message.Chat.Id.ToString(), new(message.Document.FileId, message.Document.FileSize, message.Document.FileName, message.Document.MimeType)), cToken)
                    : throw new InvalidOperationException("Document is required."),
                MessageType.Location => message.Location is not null
                    ? OnLocationHandler(requestService, new(message.Chat.Id.ToString(), new(message.Location.Latitude, message.Location.Longitude)), cToken)
                    : throw new InvalidOperationException("Location is required."),
                MessageType.Contact => message.Contact is not null
                    ? OnContactHandler(requestService, new(message.Chat.Id.ToString(), new(message.Contact.PhoneNumber, message.Contact.FirstName, message.Contact.LastName)), cToken)
                    : throw new InvalidOperationException("Contact is required."),
                _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
            };
        }
    }
    private Task HandleReceivedMessageError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        _log.ErrorCompact(exception);
        return Task.CompletedTask;
    }

    private static Task OnTextHandler(IBotRequestService service, TextEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnTextHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnPhotoHandler(IBotRequestService service, PhotoEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnPhotoHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnAudioHandler(IBotRequestService service, AudioEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnAudioHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnVideoHandler(IBotRequestService service, VideoEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnVideoHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnVoiceHandler(IBotRequestService service, VoiceEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnVoiceHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnDocumentHandler(IBotRequestService service, DocumentEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnDocumentHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnLocationHandler(IBotRequestService service, LocationEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnLocationHandler(args, cToken)
            : Task.CompletedTask;
    private static Task OnContactHandler(IBotRequestService service, ContactEventArgs args, CancellationToken cToken) =>
        !cToken.IsCancellationRequested
            ? service.OnContactHandler(args, cToken)
            : Task.CompletedTask;
}
