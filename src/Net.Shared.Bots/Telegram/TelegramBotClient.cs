using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Extensions;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using ExternalTelegramBotClient = Telegram.Bot.TelegramBotClient;

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
        var buttonsByColumns = CreateButtonsByColumns(args.Buttons.Columns, args.Buttons.Data);

        var result = new InlineKeyboardMarkup(buttonsByColumns);

        await _client.SendTextMessageAsync(args.ChatId, args.Buttons.Name, replyMarkup: result, cancellationToken: cToken);

        static List<InlineKeyboardButton[]> CreateButtonsByColumns(byte columns, Dictionary<string, string> data)
        {
            var chunkSize = (int)MathF.Ceiling(data.Count / (float)columns);

            var result = new List<InlineKeyboardButton[]>(data.Count / chunkSize + 1);

            for (var i = 0; i < data.Count; i += chunkSize)
            {
                result.Add(data.Skip(i).Take(chunkSize).Select(x => InlineKeyboardButton.WithCallbackData(x.Value, x.Key)).ToArray());
            }

            return result;
        }
    }
    public Task SendWebForm(string chatId, object data, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task SendText(string chatId, string v, CancellationToken cToken)
    {
        throw new NotImplementedException();
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

    private Task HandleSendingMessage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    private static async Task OnTextHandler(IBotRequestService requestService, TextEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnTextHandler(args);
    }
    private static async Task OnPhotoHandler(IBotRequestService requestService, PhotoEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnPhotoHandler(args);
    }
    private static async Task OnAudioHandler(IBotRequestService requestService, AudioEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnAudioHandler(args);
    }
    private static async Task OnVideoHandler(IBotRequestService requestService, VideoEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnVideoHandler(args);
    }
    private static async Task OnVoiceHandler(IBotRequestService requestService, VoiceEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnVoiceHandler(args);
    }
    private static async Task OnDocumentHandler(IBotRequestService requestService, DocumentEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnDocumentHandler(args);
    }
    private static async Task OnLocationHandler(IBotRequestService requestService, LocationEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnLocationHandler(args);
    }
    private static async Task OnContactHandler(IBotRequestService requestService, ContactEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
            await requestService.OnContactHandler(args);
    }
}
