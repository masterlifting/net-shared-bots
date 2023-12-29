using System.Collections.Immutable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Bots.Abstractions.Models.Settings;
using Net.Shared.Extensions;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using ExternalTelegramBotClient = Telegram.Bot.TelegramBotClient;

namespace Net.Shared.Bots.Telegram;

public sealed class TelegramBotClient(
        ILogger<TelegramBotClient> logger,
        IOptions<BotConnectionSettings> options,
        IServiceScopeFactory scopeFactory) : IBotClient
{
    private readonly ILogger _log = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    private readonly ITelegramBotClient _client = new ExternalTelegramBotClient(
        Environment.GetEnvironmentVariable(options.Value.TokenVariableName)
        ?? throw new InvalidOperationException($"Token with name '{options.Value.TokenVariableName}' was not found."));

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
        var file = await _client.GetInfoAndDownloadFileAsync(fileId, stream, cancellationToken: cToken);
        return stream.ToArray();
    }
    public async Task SendButtons(string chatId, Dictionary<string, string> clientButtons, CancellationToken cToken)
    {
        var buttons = new InlineKeyboardMarkup(GetButtonPairs(clientButtons.Select(x => InlineKeyboardButton.WithCallbackData(x.Value, x.Key)).ToArray()));
        
        await _client.SendTextMessageAsync(chatId, "test", replyMarkup: buttons, cancellationToken: cToken);

        static List<InlineKeyboardButton[]> GetButtonPairs(InlineKeyboardButton[] buttons)
        {
            var pairs = new List<InlineKeyboardButton[]>(buttons.Length / 2);

            for (var i = 0; i < buttons.Length; i += 2)
            {
                pairs.Add([buttons[i], buttons[i + 1]]);
            }

            return pairs;
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

            var resultTask = update.Type switch
            {
                UpdateType.Message => HandleMessage(update.Message, cToken),
                UpdateType.EditedMessage => HandleMessage(update.Message, cToken),
                UpdateType.ChannelPost => HandleMessage(update.Message, cToken),
                UpdateType.EditedChannelPost => HandleMessage(update.Message, cToken),
                UpdateType.CallbackQuery => !string.IsNullOrWhiteSpace(update.CallbackQuery?.Data)
                    ? OnTextHandler(new(update.CallbackQuery.From.Id.ToString(), new(update.CallbackQuery.Data)), cToken)
                    : throw new InvalidOperationException("Callback data is required."),
                _ => throw new NotSupportedException($"Update type {update.Type} is not supported.")
            };

            await resultTask;
        }
        catch (Exception ex)
        {
            _log.ErrorCompact(ex);
        }

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
        _log.LogError(exception, "Received message error.");
        return Task.CompletedTask;
    }
    private Task HandleSendingMessage(IBotMessage message, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    private async Task OnTextHandler(TextEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnTextHandler(args);
        }
    }
    private async Task OnPhotoHandler(PhotoEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnPhotoHandler(args);
        }
    }
    private async Task OnAudioHandler(AudioEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnAudioHandler(args);
        }
    }
    private async Task OnVideoHandler(VideoEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnVideoHandler(args);
        }
    }
    private async Task OnVoiceHandler(VoiceEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnVoiceHandler(args);
        }
    }
    private async Task OnDocumentHandler(DocumentEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnDocumentHandler(args);
        }
    }
    private async Task OnLocationHandler(LocationEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnLocationHandler(args);
        }
    }
    private async Task OnContactHandler(ContactEventArgs args, CancellationToken cToken)
    {
        if (!cToken.IsCancellationRequested)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var requestService = scope.ServiceProvider.GetRequiredService<IBotRequestService>();

            await requestService.OnContactHandler(args);
        }
    }
}
