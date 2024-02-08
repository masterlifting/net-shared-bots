using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Exceptions;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Bots.Abstractions.Models.Request;
using Net.Shared.Bots.Abstractions.Models.Response;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using static Net.Shared.Bots.Abstractions.Constants;

using ExternalTelegramBotClient = Telegram.Bot.TelegramBotClient;
using Message = Telegram.Bot.Types.Message;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(
    ILogger<TelegramBotClient> logger,
    IOptions<BotConnectionSettings> options,
    IServiceScopeFactory scopeFactory) : IBotClient
{
    private readonly ILogger _log = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ITelegramBotClient _client = new ExternalTelegramBotClient(options.Value.Token);

    public string AdminId => options.Value.AdminId;

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

    public async Task DeleteMessage(Abstractions.Models.Bot.Message message, CancellationToken cToken)
    {
        if (message.Id.HasValue)
            await _client.DeleteMessageAsync(new ChatId(message.Chat.Id), message.Id.Value, cancellationToken: cToken);
        else
            throw new InvalidOperationException("Message id is required for delete.");
    }
    
    public async Task<Result> SendText(TextEventArgs args, CancellationToken cToken)
    {
        Message response;
        switch (args.Message.ResponseBehavior)
        {
            case ResponseMessageBehavior.New:
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Text.Value, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Replace:
                if (args.Message.Id.HasValue)
                    try
                    {
                        await _client.DeleteMessageAsync(new ChatId(args.Message.Chat.Id), args.Message.Id.Value, cToken);
                    }
                    catch { }
                else
                    throw new InvalidOperationException("Message id is required for replace behavior.");
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Text.Value, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Reply:
                try
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Text.Value, replyToMessageId: args.Message.Id, cancellationToken: cToken);
                }
                catch
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Text.Value, cancellationToken: cToken);
                }
                break;
            default:
                throw new NotSupportedException($"Response message behavior {args.Message.ResponseBehavior} is not supported.");
        }

        return new(new(response.MessageId, new(args.Message.Chat.Id)));
    }
    public async Task<Result> SendButtons(ButtonsEventArgs args, CancellationToken cToken)
    {
        var columnsCount = args.Buttons.Columns != ResponseButtonsColumns.Auto
            ? (int)args.Buttons.Columns
            : args.Buttons.Data.Count % 3 == 0
                ? 3
                : args.Buttons.Data.Count % 2 == 0
                    ? 2
                    : 1;

        var buttonsByColumns = CreateByColumns(columnsCount, args.Buttons.Data);

        var request = new InlineKeyboardMarkup(buttonsByColumns);

        Message response;
        switch (args.Message.ResponseBehavior)
        {
            case ResponseMessageBehavior.New:
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Buttons.Name, replyMarkup: request, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Replace:
                if (args.Message.Id.HasValue)
                    try
                    {
                        await _client.DeleteMessageAsync(new ChatId(args.Message.Chat.Id), args.Message.Id.Value, cToken);
                    }
                    catch { }
                else
                    throw new InvalidOperationException("Message id is required for replace behavior.");
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Buttons.Name, replyMarkup: request, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Reply:
                try
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Buttons.Name, replyMarkup: request, replyToMessageId: args.Message.Id, cancellationToken: cToken);
                }
                catch
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.Buttons.Name, replyMarkup: request, cancellationToken: cToken);
                }
                break;
            default:
                throw new NotSupportedException($"Response message behavior {args.Message.ResponseBehavior} is not supported.");
        }

        return new(new(response.MessageId, new(args.Message.Chat.Id)));

        static List<InlineKeyboardButton[]> CreateByColumns(int columns, Dictionary<string, string> data)
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
    public async Task<Result> SendWebApp(WebAppEventArgs args, CancellationToken cToken)
    {
        var columnsCount = args.WebApps.Columns != ResponseButtonsColumns.Auto
            ? (int)args.WebApps.Columns
            : args.WebApps.Data.Count % 3 == 0
                ? 3
                : args.WebApps.Data.Count % 2 == 0
                    ? 2
                    : 1;

        var buttonsByColumns = CreateByColumns(columnsCount, args.WebApps.Data);

        var request = new InlineKeyboardMarkup(buttonsByColumns);

        Message response;
        switch (args.Message.ResponseBehavior)
        {
            case ResponseMessageBehavior.New:
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.WebApps.Name, replyMarkup: request, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Replace:
                if (args.Message.Id.HasValue)
                    try
                    {
                        await _client.DeleteMessageAsync(new ChatId(args.Message.Chat.Id), args.Message.Id.Value, cToken);
                    }
                    catch { }
                else
                    throw new InvalidOperationException("Message id is required for replace behavior.");
                response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.WebApps.Name, replyMarkup: request, cancellationToken: cToken);
                break;
            case ResponseMessageBehavior.Reply:
                try
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.WebApps.Name, replyMarkup: request, replyToMessageId: args.Message.Id, cancellationToken: cToken);
                }
                catch
                {
                    response = await _client.SendTextMessageAsync(args.Message.Chat.Id, args.WebApps.Name, replyMarkup: request, cancellationToken: cToken);
                }
                break;
            default:
                throw new NotSupportedException($"Response message behavior {args.Message.ResponseBehavior} is not supported.");
        }

        return new(new(response.MessageId, new(args.Message.Chat.Id)));

        static List<InlineKeyboardButton[]> CreateByColumns(int columns, Dictionary<string, Uri> data)
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

    private async Task HandleReceivedMessage(ITelegramBotClient client, Update? update, CancellationToken cToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(update, "Received data was not recognized.");

            await using var scope = _scopeFactory.CreateAsyncScope();

            var request = scope.ServiceProvider.GetRequiredService<IBotRequest>();

            var result = update.Type switch
            {
                UpdateType.Message => HandleMessage(request, update.Message, cToken),
                UpdateType.EditedMessage => HandleMessage(request, update.Message, cToken),
                UpdateType.ChannelPost => HandleMessage(request, update.Message, cToken),
                UpdateType.EditedChannelPost => HandleMessage(request, update.Message, cToken),
                UpdateType.CallbackQuery => !string.IsNullOrWhiteSpace(update.CallbackQuery?.Data)
                    ? OnTextHandler(request, new(new(update.CallbackQuery.Message?.MessageId, new(update.CallbackQuery.From.Id.ToString())), new(update.CallbackQuery.Data)), cToken)
                    : throw new InvalidOperationException("Callback data is required."),
                _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
            };

            await result;
        }
        catch (Exception exception)
        {
            await HandleReceivedMessageError(client, exception, cToken);
        }

        Task HandleMessage(IBotRequest request, Message? message, CancellationToken cToken)
        {
            ArgumentNullException.ThrowIfNull(message, "Received data was not recognized.");

            return message.Type switch
            {
                MessageType.Text => !string.IsNullOrWhiteSpace(message.Text)
                    ? OnTextHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Text)), cToken)
                    : throw new InvalidOperationException("Text is required."),
                MessageType.Photo => message.Photo is not null
                    ? OnPhotoHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), message.Photo.Select(x => new Photo(x.FileId, x.FileSize)).ToImmutableArray()), cToken)
                    : throw new InvalidOperationException("Photo is required."),
                MessageType.Audio => message.Audio is not null
                    ? OnAudioHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Audio.FileId, message.Audio.FileSize, message.Audio.Title, message.Audio.MimeType)), cToken)
                    : throw new InvalidOperationException("Audio is required."),
                MessageType.Video => message.Video is not null
                    ? OnVideoHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Video.FileId, message.Video.FileSize, message.Video.FileName, message.Video.MimeType)), cToken)
                    : throw new InvalidOperationException("Video is required."),
                MessageType.Voice => message.Voice is not null
                    ? OnVoiceHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Voice.FileId, message.Voice.FileSize, message.Voice.MimeType)), cToken)
                    : throw new InvalidOperationException("Voice is required."),
                MessageType.Document => message.Document is not null
                    ? OnDocumentHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Document.FileId, message.Document.FileSize, message.Document.FileName, message.Document.MimeType)), cToken)
                    : throw new InvalidOperationException("Document is required."),
                MessageType.Location => message.Location is not null
                    ? OnLocationHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Location.Latitude, message.Location.Longitude)), cToken)
                    : throw new InvalidOperationException("Location is required."),
                MessageType.Contact => message.Contact is not null
                    ? OnContactHandler(request, new(new(message.MessageId, new(message.Chat.Id.ToString())), new(message.Contact.PhoneNumber, message.Contact.FirstName, message.Contact.LastName)), cToken)
                    : throw new InvalidOperationException("Contact is required."),
                _ => throw new NotSupportedException($"Message type {message.Type} is not supported.")
            };
        }
    }
    private Task HandleReceivedMessageError(ITelegramBotClient client, Exception exception, CancellationToken cToken)
    {
        exception = exception.InnerException ?? exception;

        _log.ErrorFull(exception);

        return client.SendTextMessageAsync(AdminId, exception.Message, cancellationToken: cToken);
    }

    private Task OnTextHandler(IBotRequest request, TextEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleText, args.Message, args, cToken);
    private Task OnPhotoHandler(IBotRequest request, PhotoEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandlePhoto, args.Message, args, cToken);
    private Task OnAudioHandler(IBotRequest request, AudioEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleAudio, args.Message, args, cToken);
    private Task OnVideoHandler(IBotRequest request, VideoEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleVideo, args.Message, args, cToken);
    private Task OnVoiceHandler(IBotRequest request, VoiceEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleVoice, args.Message, args, cToken);
    private Task OnDocumentHandler(IBotRequest request, DocumentEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleDocument, args.Message, args, cToken);
    private Task OnLocationHandler(IBotRequest request, LocationEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleLocation, args.Message, args, cToken);
    private Task OnContactHandler(IBotRequest request, ContactEventArgs args, CancellationToken cToken) =>
        OnRequestMessageHandler(request.HandleContact, args.Message, args, cToken);

    private async Task OnRequestMessageHandler<T>(Func<T, CancellationToken, Task> handler, Abstractions.Models.Bot.Message message, T data, CancellationToken cToken) where T : class
    {
        if (!cToken.IsCancellationRequested)
            try
            {
                await handler(data, cToken);
            }
            catch (UserInvalidOperationException exception)
            {
                await SendText(new(message, new(exception.Message)), cToken);
                return;
            }
            catch
            {
                throw;
            }
    }
}
