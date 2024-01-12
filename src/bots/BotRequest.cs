using Net.Shared.Abstractions.Models.Exceptions;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

using static Net.Shared.Bots.Abstractions.Constants;

namespace Net.Shared.Bots;

internal sealed class BotRequest(
    IBotClient botClient,
    IBotCommandsStore botCommandsStore,
    IBotResponse botResponse) : IBotRequest
{
    private readonly IBotClient _botClient = botClient;
    private readonly IBotCommandsStore _botCommandsStore = botCommandsStore;
    private readonly IBotResponse _botResponse = botResponse;

    public async Task HandleText(TextEventArgs args, CancellationToken cToken)
    {
        if (args.Text.Value.StartsWith('/'))
        {
            var commandName = args.Text.Value.TrimStart('/');

            var parametersStartIndex = commandName.IndexOf('?');

            var commandParameters = new Dictionary<string, string>(5);

            if (parametersStartIndex > 2)
            {
                commandName = commandName[..parametersStartIndex];

                var parameters = args.Text.Value[(parametersStartIndex + 2)..].Split('&', StringSplitOptions.RemoveEmptyEntries);

                foreach (var parameter in parameters)
                {
                    var keyValue = parameter.Split('=', StringSplitOptions.RemoveEmptyEntries);

                    if (keyValue.Length != 2)
                        throw new NotSupportedException($"The parameter '{parameter}' is not supported.");

                    commandParameters.Add(keyValue[0], keyValue[1]);
                }
            }

            var command = await _botCommandsStore.Create(args.Chat.Id, commandName, commandParameters, cToken);

            try
            {
                await _botResponse.Create(args.Chat.Id, command, cToken);
            }
            catch (UserInvalidOperationException exception)
            {
                var messageArgs = new MessageEventArgs(args.Chat.Id, new(exception.Message));
                await _botClient.SendMessage(messageArgs, cToken);
                return;
            }
            catch
            {
                var messageArgs = new MessageEventArgs(args.Chat.Id, new(Shared.Abstractions.Constants.UserErrorMessage));
                await _botClient.SendMessage(messageArgs, cToken);

                throw;
            }

        }
        else if (args.Text.Value.StartsWith('\"') && args.Text.Value.EndsWith('\"'))
        {
            var commandName = Commands.Ask;

            var text = $"ChatId: {args.Chat.Id}, Message: {args.Text.Value.Trim('\"')}";

            var commandParameters = new Dictionary<string, string>
            {
                { CommandParameters.Message, text }
            };
            
            var command = await _botCommandsStore.Create(args.Chat.AdminId, commandName, commandParameters, cToken);

            await _botResponse.Create(args.Chat.AdminId, command, cToken);
        }
        else if (Guid.TryParse(args.Text.Value, out var guid))
        {
            var command = await _botCommandsStore.Get(args.Chat.Id, guid, cToken);

            try
            {
                await _botResponse.Create(args.Chat.Id, command, cToken);
            }
            catch (UserInvalidOperationException exception)
            {
                var messageArgs = new MessageEventArgs(args.Chat.Id, new(exception.Message));
                await _botClient.SendMessage(messageArgs, cToken);
                return;
            }
            catch
            {
                var messageArgs = new MessageEventArgs(args.Chat.Id, new(Shared.Abstractions.Constants.UserErrorMessage));
                await _botClient.SendMessage(messageArgs, cToken);

                throw;
            }
        }
        else
            throw new NotSupportedException($"The message '{args.Text.Value}' is not supported.");
    }
    public Task HandlePhoto(PhotoEventArgs photo, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleAudio(AudioEventArgs audio, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleVideo(VideoEventArgs video, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleVoice(VoiceEventArgs voice, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleDocument(DocumentEventArgs document, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleLocation(LocationEventArgs location, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task HandleContact(ContactEventArgs contact, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
